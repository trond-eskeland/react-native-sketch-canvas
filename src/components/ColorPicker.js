/* eslint-disable react/prefer-stateless-function */
import React, { Component } from 'react';
import { Text, View, TouchableOpacity, StyleSheet } from 'react-native';


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
        <TouchableOpacity
          key={item.color}
          onPress={() => this.props.onColorChange(item)}
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
}

export default ColorBar;
