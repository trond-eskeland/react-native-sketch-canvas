import React, { Component } from 'react';
import { View, StyleSheet, Image } from 'react-native';
import Button from '../components/Button';
import ColorPicker from '../components/ColorPicker';
import FormPicker from '../components/FormPicker';

import brush from '../../resources/images/brush.png'
import brushActive from '../../resources/images/brush_active.png';
import title from '../../resources/images/title.png';
import titleActive from '../../resources/images/title_active.png';
import undo from '../../resources/images/undo.png';
import move from '../../resources/images/move.png';
import moveActive from '../../resources/images/move_active.png';
import arrow from '../../resources/images/arrow.png';
import arrowActive from '../../resources/images/arrow_active.png';

import rectangle from '../../resources/images/rectangle.png';
import rectangleActive from '../../resources/images/rectangle_active.png';
import ellipseActive from '../../resources/images/ellipse_active.png';
import cloudActive from '../../resources/images/cloud_active.png';

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

class ToolBar extends Component {

  state = {
    optionToolbarType: '',
  }

  renderOptionBar() {
      switch (this.state.optionToolbarType) {
        case 'color':
          return (<ColorPicker onPress={strokeColor => this.props.onColorChange(strokeColor)} />);
        case 'form':
          return (<FormPicker drawingMode={this.props.drawingMode} onPress={mode => this.props.onPress(mode)} />);
        default:
          break;
      }

      return null;
  }

  getFormIcon() {
    switch (this.props.drawingMode) {
      case 'form-rectangle':
        return rectangleActive;
      case 'form-ellipse':
        return ellipseActive;
      case 'form-cloud':
        return cloudActive;
      default:
        return rectangle
    }
  }

  render() {
    const { 
        drawingMode,
        strokeColor,
        onPress,
        onUndo,
    } = this.props;

    return (
        <View style={{ width: '100%', backgroundColor: '#333333' }}>
            {this.renderOptionBar()}
            <View style={[styles.toolBar, !this.state.optionToolbarType && styles.toolBarBorder]}>
                <Button onPress={() => onPress('zoom')}>
                    <Image source={drawingMode === 'zoom' ? moveActive : move} />
                </Button>
                <Button onPress={() => onPress('line')}>
                    <Image source={drawingMode === 'line' ? brushActive : brush} />
                </Button>
                <Button onPress={() => this.setState({ optionToolbarType: this.state.optionToolbarType === 'form' ? '' : 'form' })}>
                    <Image source={this.getFormIcon()} />
                </Button>
                <Button onPress={() => onPress('text')}>
                    <Image source={drawingMode === 'text' ? titleActive : title} />
                </Button>
                <Button onPress={() => onPress('arrow')}>
                    <Image source={drawingMode === 'arrow' ? arrowActive : arrow} />
                </Button>
                <Button onPress={() => this.setState({ optionToolbarType: this.state.optionToolbarType === 'color' ? '' : 'color' })}>
                    <View style={[{ backgroundColor: strokeColor.color, borderWidth: strokeColor.border ? 1 : 0 }, styles.strokeColorButton]} />
                </Button>
                <Button onPress={() => onUndo()}>
                    <Image source={undo} />
                </Button>
            </View>
        </View>
    );
  }
}

export default ToolBar;
