import React, { Component } from 'react';
import { View, StyleSheet } from 'react-native';
import OptionBar from './OptionBar'
import Button from './Button';

const styles = StyleSheet.create({
  strokeColorButton: {
    marginHorizontal: 2.5, marginVertical: 8, width: 30, height: 30, borderRadius: 15,
  },
});

export class ColorBar extends Component {
  static defaultProps = {
    strokeColors: [
      { color: '#FF3B3B' },
      { color: '#FBCA36' },
      { color: '#1273DD' },
      { color: '#4BB748' },
      { color: '#000000' },
      { color: '#FFFFFF', border: true }],
  };

  render() {
    const colors = this.props.strokeColors
      .map(item => (
        <Button
          key={item.color}
          onPress={() => this.props.onPress(item)}
        >
          <View style={[{ backgroundColor: item.color, borderWidth: item.border ? 1 : 0 }, styles.strokeColorButton]} />
        </Button>
      ));
    return (
      <OptionBar>
        {colors}
      </OptionBar>
    );
  }
}

export default ColorBar;
