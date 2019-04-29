import React, { Component } from 'react';
import { View, StyleSheet } from 'react-native';

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

const OptionBar = (props) => {
  return (
    <View style={[styles.toolBar, styles.toolBarBorder]}>
      {props.children}
    </View>
  );
};

export default OptionBar;
