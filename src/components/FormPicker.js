/* eslint-disable react/prefer-stateless-function */
import React, { Component } from 'react';
import { Image } from 'react-native';
import OptionBar from './OptionBar';
import Button from './Button';

import rectangle from '../../resources/images/rectangle.png';
import rectangleActive from '../../resources/images/rectangle_active.png';
import ellipse from '../../resources/images/ellipse.png';
import ellipseActive from '../../resources/images/ellipse_active.png';
import cloud from '../../resources/images/cloud.png';
import cloudActive from '../../resources/images/cloud_active.png';


class FormPicker extends Component {
  render() {
    const { drawingMode, onPress} = this.props;

    return (
      <OptionBar>
        <Button onPress={() => onPress('form-rectangle')}>
          <Image source={drawingMode === 'form-rectangle' ? rectangleActive : rectangle} />
        </Button>
        <Button onPress={() => onPress('form-ellipse')}>
          <Image source={drawingMode === 'form-ellipse' ? ellipseActive : ellipse} />
        </Button>
        <Button onPress={() => onPress('form-cloud')}>
          <Image source={drawingMode === 'form-cloud' ? cloudActive : cloud} />
        </Button>
      </OptionBar>
    );
  }
}

export default FormPicker;
