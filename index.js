import React, { Component } from 'react';
import {
  Image,
  Text,
  View,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  SafeAreaView,
  PixelRatio,
  Platform,
  Keyboard,
  Dimensions,
  PanResponder,
  Animated,
  ActivityIndicator,
} from 'react-native';
import SketchCanvas from './src/SketchCanvas';
import PropTypes from 'prop-types';
import ToolBar from './src/components/ToolBar';
import ResponsiveView from './src/components/ResponsiveView';
import move from './resources/images/move.png';

const styles = StyleSheet.create({
  welcome: {
    fontSize: 20,
    textAlign: 'center',
    margin: 10,
  },
  instructions: {
    textAlign: 'center',
    color: '#333333',
    marginBottom: 5,
  },
  container: {
    flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#F5FCFF',
  },
  strokeColorButton: {
    marginHorizontal: 2.5, marginVertical: 8, width: 30, height: 30, borderRadius: 15,
  },
  strokeWidthButton: {
    marginHorizontal: 2.5,
    marginVertical: 8,
    width: 30,
    height: 30,
    borderRadius: 15,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#39579A',
  },
  functionButton: {
    marginHorizontal: 2.5,
    marginVertical: 8,
    height: 40,
    width: 40,
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 5,
  },
  toolBar: {
    flexDirection: 'row',
    width: '100%',
    backgroundColor: '#FFFFFF',
    alignItems: 'center',
    justifyContent: 'space-evenly',
  },
  toolBarBorder: {
    borderTopLeftRadius: 10,
    borderTopRightRadius: 10,
    borderWidth: 0.4,
    borderBottomWidth: 0,
  },
  textEditContainer: {
    position: 'absolute',
    left: 0,
    top: 0,
    width: '100%',
    backgroundColor: 'rgba(0,0,0,0.5)',
  },
  textEditHorizontalContainer: {
    flexDirection: 'row',
    paddingBottom: 0,
    paddingRight: 30,
  },
});

const { height, width } = Dimensions.get('window');

let canvas = null;

export default class RNSketchCanvas extends React.Component {
  static propTypes = {
    maxZoom: PropTypes.number,
    minZoom: PropTypes.number,
    scrollEnabled: PropTypes.bool,
    strokeColors: PropTypes.arrayOf(PropTypes.shape({ color: PropTypes.string })),
    imageTextDefault: PropTypes.object,
    requiredTouches: PropTypes.number,
    image: PropTypes.object,
    onSketchSaved: PropTypes.func,
  };

  static defaultProps = {
    strokeColors: [
      { color: '#FF3B3B' },
      { color: '#FBCA36' },
      { color: '#1273DD' },
      { color: '#4BB748' },
      { color: '#000000' },
      { color: '#FFFFFF', border: true }],
    imageTextDefault: {
      text: '',
      fontColor: '#FF3B3B',
      fontSize: 15,
      anchor: { x: 0, y: 0 },
      renderPosition: { x: 0, y: 0 },
      position: { x: 0, y: 0 },
      mode: 'none',
    },
    maxZoom: 1.5,
    minZoom: 0.2,
    scrollEnabled: true,
    requiredTouches: null,
    onSketchSaved: () => {},
  };


  constructor(props) {
    super(props);
    this.screenScale = Platform.OS === 'ios' ? 1 : PixelRatio.get();
  }

  state = {
    pan: new Animated.ValueXY(),
    strokeWidth: 4,
    strokeColor: { color: 'red' },
    imageText: [],
    imageTextCurrent: {
      text: '',
      fontColor: '#FF3B3B',
      fontSize: 15,
      anchor: { x: 0, y: 0 },
      renderPosition: { x: 0, y: 0 },
      position: { x: 0, y: 0 },
      mode: 'none',
    },
    drawStep: [],
    showColorPicker: false,
    touchEnabled: true,
    drawingMode: 'line',
    zoomOffset: null,
  }

  componentWillMount() {
    this.animatedValueX = 0;
    this.animatedValueY = 0;
    this.state.pan.x.addListener((value) => { this.animatedValueX = value.value; });
    this.state.pan.y.addListener((value) => { this.animatedValueY = value.value; });

    this.panResponder = PanResponder.create({
      onMoveShouldSetResponderCapture: () => true,
      onMoveShouldSetPanResponderCapture: () => true,
      onPanResponderGrant: () => {
        this.state.pan.setOffset({ x: this.animatedValueX, y: this.animatedValueY });
        this.state.pan.setValue({ x: 0, y: 0 });
      },
      onPanResponderMove: (event, gesture) => {
        this.state.pan.setValue({ x: gesture.dx, y: gesture.dy });
        this.editText(this.animatedValueX, this.animatedValueY);
      },
      onPanResponderRelease: () => {
        this.state.pan.flattenOffset();
      },
    });

    if (this.props.image) {
			//this.getBackgroundImageSize(image);
		} else {
			console.warn('did not try to get image ', this.props);
    }
  }


  componentDidMount() {
  }

  componentWillReceiveProps(nextProps) {
    if (nextProps.image !== this.props.image) {

    }
  }

  componentDidUpdate(prevProps, prevState) {
    if (prevState.imageTextCurrent.mode !== 'edit' && this.state.imageTextCurrent.mode === 'edit') {
      if (this.textInput) this.textInput.focus();
    }
  }

  componentWillUnmount() {
    this.state.pan.x.removeAllListeners();
    this.state.pan.y.removeAllListeners();
  }

  onSketchSaved(success, path) {
    /*
    const {
      attachment,
      notificationId,
      onAddAttachment,
    } = this.props.navigation.state.params;

    if (success) {
      const uri = `file://${path}`;
      const sketchAttachment = Object.assign({}, attachment);
      sketchAttachment.uri = uri;

      onAddAttachment({ notificationId, attachment: sketchAttachment });
    }
    */
  }

  onColorChange(strokeColor) {
    this.setState(({ strokeColor, showColorPicker: false }));
    if (this.state.imageTextCurrent !== null) {
      this.editText(undefined, undefined, undefined, strokeColor.color);
    }
  }

  setDrawMode(mode) {
    if (this.state.imageTextCurrent.mode === 'move') {
      this.saveText();
    }

    if (mode === 'line') {
      this.setState({ touchEnabled: true, imageTextCurrent: this.props.imageTextDefault, drawingMode: mode });
      canvas.lockViewPort(false);
    } else if (mode === 'text') {
      if (this.state.imageTextCurrent.mode !== 'move') {
        const item = Object.assign({}, this.state.imageTextCurrent);
        item.mode = 'edit';
        this.setState({ touchEnabled: false, imageTextCurrent: item, drawingMode: mode });
      }
    } else if (mode.includes('form')) {
      this.setState({ touchEnabled: false, imageTextCurrent: this.props.imageTextDefault, drawingMode: mode });
    } else {
      this.setState({ touchEnabled: true, imageTextCurrent: this.props.imageTextDefault, drawingMode: mode });
      canvas.lockViewPort(true);
    }
  }

  saveCanvas = () => {
    const filename = String(Math.ceil(Math.random() * 100000000));
    canvas.save('jpg', true, 'Documents/attachments', `${filename}.jpg`, true, true, false);
  };

  save() {
    const filename = String(Math.ceil(Math.random() * 100000000));
    canvas.save('jpg', true, 'Documents/attachments', `${filename}.jpg`, true, true, false);
  }

  confirm() {
    if (this.state.imageTextCurrent.mode === 'move') {
      this.saveText(() => this.save());
    } else {
      this.saveCanvas();
    }
  }

  saveText(callback) {
    const item = Object.assign({}, this.state.imageTextCurrent);
    item.fontSize = 15 * this.screenScale;
    item.mode = 'none';

    let { x, y } = item.position;

    const { zoomOffset } = this.state;

    if (zoomOffset) {
      x = ((x * zoomOffset.screenImageRatioWidth) + zoomOffset.horizontalOffset) / zoomOffset.zoomFactor;
      y = ((y * zoomOffset.screenImageRatioHeight) + zoomOffset.verticalOffset) / zoomOffset.zoomFactor;
      item.position.x = x;
      item.position.y = y;
    }

    const imageText = [...this.state.imageText, item];
    this.setState({ imageText, imageTextCurrent: this.props.imageTextDefault, drawingMode: 'none' }, () => callback());
    this.addDrawStep('text');
  }

  editText(x, y, text, color, mode) {
    const item = Object.assign({}, this.state.imageTextCurrent);
    item.fontColor = this.state.strokeColor.color;
    if (x && y) item.renderPosition = { x, y };
    if (x && y) item.position = { x: x * this.screenScale, y: y * this.screenScale };
    if (text !== undefined) item.text = text;
    if (color !== undefined) item.fontColor = color;
    if (mode !== undefined) item.mode = mode;

    if (mode === 'move') {
      if (item.text === '') {
        item.mode = 'none';
      }
      Keyboard.dismiss();
      item.position = { x: (width / 2.5) * this.screenScale, y: (height / 2.5) * this.screenScale };
      Animated.spring(this.state.pan, { toValue: { x: item.position.x, y: item.position.y } }).start();
    }

    this.setState({ imageTextCurrent: item });
  }

  undoText() {
    const imageText = [...this.state.imageText];
    imageText.pop();
    this.setState({ imageText });
  }

  addTextCanceled() {
    this.setState({ imageTextCurrent: this.props.imageTextDefault, drawingMode: 'none' });
  }

  saveSketch() {
    const filename = String(Math.ceil(Math.random() * 100000000));
    this.canvas.save('jpg', true, 'Documents/attachments', `${filename}.jpg`, true, true, false);
  }

  addDrawStep(func) {
    const items = [...this.state.drawStep, { type: func }];
    this.setState({ drawStep: items });
  }

  undoDrawStep() {
    if (this.state.imageTextCurrent.mode === 'move') {
      this.addTextCanceled();
      return;
    }

    const items = [...this.state.drawStep];
    if (items.length > 0) {
      const lastItem = items.pop();
      if (lastItem.type === 'line') {
        canvas.undo();
      } else if (lastItem.type === 'text') {
        this.undoText();
      }
      this.setState({ drawStep: items });
    }
  }

  updateZoomLevel(zoom) {
    this.setState({ zoom });
  }

  renderSpinner() {
    return (
      <View style={{ flex: 1, alignSelf: 'center', justifyContent: 'center' }}>
        <ActivityIndicator size={'large'} />
      </View>
    );
  }

  render() {
    const {
      color,
      border,
    } = this.state.strokeColor;
    const {
      drawingMode,
      strokeColor,
      showColorPicker,
      zoomOffset,
    } = this.state;

    const fontSize = zoomOffset ?
      ((this.state.imageTextCurrent.fontSize * zoomOffset.zoomFactor) / zoomOffset.screenImageRatioHeight)
      : 15;

    const file = this.props.image && this.props.image.uri.replace('file://', '');

    if (!this.state.contentStyle) {
      // return this.renderSpinner();
    }

    return (
      <View style={{ flex: 1, backgroundColor: '#FFFFFF' }} behavior="position">
        <SafeAreaView style={{ flex: 1 }}>
          <View style={{ flex: 1, backgroundColor: '#333333' }}>
            <SketchCanvas
              ref={(ref) => { canvas = ref; }}
              style={{ flex: 1, marginBottom: this.state.showColorPicker ? 0 : 39 }}
              strokeColor={color}
              strokeWidth={this.state.strokeWidth}
              onSketchSaved={(success, path) => this.props.onSketchSaved(success, path)}
              text={this.state.imageText}
              localSourceImage={file && { filename: file, mode: 'AspectFit' }}
              touchEnabled={this.state.touchEnabled}
              onDisabledTouch={(x, y) => this.editText(x, y)}
              onStrokeEnd={() => this.addDrawStep('line')}
              onZoomChange={zoomOffset => this.setState({ zoomOffset })}
              requiredTouches={1}
              scale={this.state.zoom}
            />
            { this.state.imageTextCurrent.mode === 'edit' &&
            <View style={styles.textEditContainer}>
              <View style={styles.textEditHorizontalContainer}>
                <TouchableOpacity onPress={() => this.editText(null, null, undefined, undefined, 'move')} >
                  <Text style={{ color: 'white', padding: 10 }}>OK</Text>
                </TouchableOpacity>
                <View style={{ justifyContent: 'center', alignItems: 'center' }}> 
                  <TextInput
                    style={{
                      color: 'white',
                      backgroundColor: 'rgba(0,0,0,0.1)',
                      fontSize: this.state.imageTextCurrent.fontSize,
                    }}
                    ref={(ref) => { this.textInput = ref; }}
                    onChangeText={text => this.editText(null, null, text)}
                    multiline
                    value={this.state.imageTextCurrent.text}
                  />
                </View>

              </View>
            </View>
            }
            { this.state.imageTextCurrent.mode === 'move' &&
              <View style={{
                position: 'absolute',
                top: 0,
                left: 0,
                }}
              >
                <Animated.View
                  {...this.panResponder.panHandlers}
                  style={[this.state.pan.getLayout()]}
                >
                  <Image source={move} style={{ position: 'absolute', top: -20, left: -20 }} />
                  <Text
                    style={{
                      backgroundColor: 'rgba(0,0,0,0.1)',
                      color: this.state.imageTextCurrent.fontColor,
                      fontSize: fontSize,
                      padding: 5,
                    }}
                    multiline
                  >
                    {this.state.imageTextCurrent.text}
                  </Text>
                </Animated.View>
              </View>
            }
          </View>
          <View style={{ width: '100%', backgroundColor: 'rgba(0,0,0,0)' }}>
            <ToolBar
              onPress={mode => this.setDrawMode(mode)}
              onColorChange={selectedColor => this.onColorChange(selectedColor)}
              onUndo={() => this.undoDrawStep()}
              showColorPicker={showColorPicker}
              strokeColor={strokeColor}
              drawingMode={drawingMode}
            />
          </View>
        </SafeAreaView>
      </View>
    );
  }
}

RNSketchCanvas.MAIN_BUNDLE = SketchCanvas.MAIN_BUNDLE;
RNSketchCanvas.DOCUMENT = SketchCanvas.DOCUMENT;
RNSketchCanvas.LIBRARY = SketchCanvas.LIBRARY;
RNSketchCanvas.CACHES = SketchCanvas.CACHES;

export { SketchCanvas };
