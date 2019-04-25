import React, { Component } from 'react';
import { Text, View, StyleSheet, Image } from 'react-native';
import Button from '../components/Button';
import ColorPicker from '../components/ColorPicker';

import brush from '../../resources/images/brush.png'
import brushActive from '../../resources/images/brush_active.png';
import title from '../../resources/images/title.png';
import titleActive from '../../resources/images/title_active.png';
import undo from '../../resources/images/undo.png';
import move from '../../resources/images/move.png';

const styles = StyleSheet.create({
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
    strokeColorButton: {
        marginHorizontal: 2.5, marginVertical: 8, width: 30, height: 30, borderRadius: 15,
    },
  });

export class ToolBar extends Component {

   state = {
    showColorPicker: false,
   }

    onColorChange(strokeColor) {
        this.setState({ showColorPicker: !this.state.showColorPicker })
        this.props.onColorChange(strokeColor);
    }

  render() {
    const { 
        drawingMode,
        strokeColor,
        onPress,
        onUndo,
        onColorChange,
        showColorPicker
    } = this.props;



    return (
        <View style={{ width: '100%', backgroundColor: '#333333' }}>
            {!!this.state.showColorPicker && <ColorPicker onColorChange={strokeColor => this.onColorChange(strokeColor)} />}
            <View style={[styles.toolBar, !this.state.showColorPicker && styles.toolBarBorder]}>
                <Button onPress={() => onPress('line')}>
                    <Image source={drawingMode === 'line' ? brushActive : brush} />
                </Button>
                <Button onPress={() => onPress('text')}>
                    <Image source={drawingMode === 'text' ? titleActive : title} />
                </Button>
                <Button onPress={() => this.setState({ showColorPicker: !this.state.showColorPicker })}>
                    <View style={[{ backgroundColor: strokeColor.color, borderWidth: strokeColor.border ? 1 : 0 }, styles.strokeColorButton]} />
                </Button>
                <Button onPress={() => onUndo()}>
                    <Image source={undo} />
                </Button>
              {/* {this.renderButton({ jsx: () => <Image source={drawingMode === 'line' ? brushActive : brush} />, onPress: () => this.setDrawMode('line') })}
              {this.renderButton({ jsx: () => <Image source={drawingMode === 'text' ? titleActive : title} />, onPress: () => this.setDrawMode('text') })}
              {this.renderButton({ jsx: () => <View style={[{ backgroundColor: color, borderWidth: border ? 1 : 0 }, styles.strokeColorButton]} />, onPress: () => this.setState({ showColorPicker: !this.state.showColorPicker }) })}
              {this.renderButton({ jsx: () => <Image source={undo} />, onPress: () => this.undoDrawStep() })} */}
            </View>
        </View>
    );
  }
}

export default ToolBar;
