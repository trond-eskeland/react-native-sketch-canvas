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
} from 'react-native';
import SketchCanvas from './src/SketchCanvas'
import PropTypes from 'prop-types';


import brush from './resources/images/brush.png';
import brushActive from './resources/images/brush_active.png';
import title from './resources/images/title.png';
import titleActive from './resources/images/title_active.png';
import undo from './resources/images/undo.png';
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
    strokeColors: PropTypes.arrayOf(PropTypes.shape({ color: PropTypes.string })),
    notificationId: PropTypes.string,
    imageTextDefault: PropTypes.object,
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
    notificationId: '',
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
    keyboardHeight: height / 2,

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
  }


  componentDidMount() {
    this.keyboardDidShowListener = Keyboard.addListener(
      'keyboardDidShow',
      this.keyboardDidShow.bind(this),
    );
  }

  componentWillReceiveProps(nextProps) {

  }

  componentDidUpdate(prevProps, prevState) {
    if (prevState.imageTextCurrent.mode !== 'edit' && this.state.imageTextCurrent.mode === 'edit') {
      if (this.textInput) this.textInput.focus();
    }
  }

  componentWillUnmount() {
    this.keyboardDidShowListener.remove();
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

  setDrawMode(mode) {
    if (this.state.imageTextCurrent.mode === 'move') {
      this.saveText();
    }

    if (mode === 'line') {
      this.setState({ touchEnabled: true, imageTextCurrent: this.props.imageTextDefault, drawingMode: mode });
    } else if (mode === 'text') {
      if (this.state.imageTextCurrent.mode !== 'move') {
        const item = Object.assign({}, this.state.imageTextCurrent);
        item.mode = 'edit';
        this.setState({ touchEnabled: false, imageTextCurrent: item, drawingMode: mode });
      }
    }
  }
  saveCanvas = () => {
    const filename = String(Math.ceil(Math.random() * 100000000));
    canvas.save('jpg', true, 'Documents/attachments', `${filename}.jpg`, true, true, false);
    //this.props.navigation.goBack();
  };


  confim() {
    if (this.state.imageTextCurrent.mode === 'move') {
      this.saveText(this.saveCanvas.bind(this));
    } else {
      this.saveCanvas();
    }
  }

  saveText(callback) {
    const item = Object.assign({}, this.state.imageTextCurrent);
    item.fontSize = 15 * this.screenScale;
    item.mode = 'none';

    const imageText = [...this.state.imageText, item];
    this.setState({ imageText, imageTextCurrent: this.props.imageTextDefault, drawingMode: 'none' }, () => callback);
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

  keyboardDidShow(e) {
    this.setState({ keyboardHeight: e.endCoordinates.height });
  }

  saveSketch() {
    const filename = String(Math.ceil(Math.random() * 100000000));
    canvas.save('jpg', true, 'Documents/attachments', `${filename}.jpg`, true, true, false);
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

  renderColorItem = ({ item }) => (
    <TouchableOpacity
      style={{ marginHorizontal: 2.5 }}
      onPress={() => { this.setState({ strokeColor: item, showColorPicker: false }); }}
    >
      <View style={[{ backgroundColor: item.color, borderWidth: item.border ? 1 : 0 }, styles.strokeColorButton]} />
    </TouchableOpacity>
  )

  renderColorBar() {
    const colors = this.props.strokeColors
      .map(item => (
        <TouchableOpacity
          key={item.color}
          onPress={() => {
            this.setState(({ strokeColor: item, showColorPicker: false }));
            if (this.state.imageTextCurrent !== null) {
              this.editText(undefined, undefined, undefined, item.color);
            }
          }}
        >
          <View style={[{ backgroundColor: item.color, borderWidth: item.border ? 1 : 0 }, styles.strokeColorButton]} />
        </TouchableOpacity>
      ));
    return (
      <View style={[styles.toolBar, styles.toolBarBorder]}>
        {colors}
      </View>
    );
  }

  renderButton(data) {
    return (
      <TouchableOpacity onPress={() => data.onPress()}>
        <View style={styles.functionButton}>
          {data.jsx && data.jsx()}
        </View>
      </TouchableOpacity>
    );
  }

  render() {
    // const {
    //   attachment,
    // } = this.props.navigation.state.params;
    const {
      color,
      border,
    } = this.state.strokeColor;
    const {
      drawingMode,
    } = this.state;

    const file = null; // attachment.uri.replace('file://', '');

    return (
      <View style={{ flex: 1, backgroundColor: '#FFFFFF' }} behavior="position">
        <SafeAreaView style={{ flex: 1 }}>
          <View style={{ flex: 1, backgroundColor: '#333333' }}>
            <SketchCanvas
              ref={(ref) => { canvas = ref; }}
              style={{ flex: 1, marginBottom: this.state.showColorPicker ? 0 : 49 }}
              strokeColor={color}
              strokeWidth={this.state.strokeWidth}
              onSketchSaved={(success, path) => this.onSketchSaved(success, path)}
              text={this.state.imageText}
              localSourceImage={file && { filename: file, mode: 'AspectFit' }}
              touchEnabled={this.state.touchEnabled}
              onDisabledTouch={(x, y) => this.editText(x, y)}
              onStrokeEnd={() => this.addDrawStep('line')}
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
                      fontSize: this.state.imageTextCurrent.fontSize,
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
          <View style={{ width: '100%', backgroundColor: '#333333' }}>
            {!!this.state.showColorPicker && this.renderColorBar()}
            <View style={[styles.toolBar, !this.state.showColorPicker && styles.toolBarBorder]}>
              {this.renderButton({ jsx: () => <Image source={drawingMode === 'line' ? brushActive : brush} />, onPress: () => this.setDrawMode('line') })}
              {this.renderButton({ jsx: () => <Image source={drawingMode === 'text' ? titleActive : title} />, onPress: () => this.setDrawMode('text') })}
              {this.renderButton({ jsx: () => <View style={[{ backgroundColor: color, borderWidth: border ? 1 : 0 }, styles.strokeColorButton]} />, onPress: () => this.setState({ showColorPicker: !this.state.showColorPicker }) })}
              {this.renderButton({ jsx: () => <Image source={undo} />, onPress: () => this.undoDrawStep() })}
            </View>
          </View>
          {false && this.renderDraggable()}
        </SafeAreaView>
      </View>
    );
  }

};

RNSketchCanvas.MAIN_BUNDLE = SketchCanvas.MAIN_BUNDLE;
RNSketchCanvas.DOCUMENT = SketchCanvas.DOCUMENT;
RNSketchCanvas.LIBRARY = SketchCanvas.LIBRARY;
RNSketchCanvas.CACHES = SketchCanvas.CACHES;

export {
  SketchCanvas
}