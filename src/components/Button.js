import React, { Component } from 'react';
import { TouchableOpacity, View, StyleSheet } from 'react-native';

const styles = StyleSheet.create({
  functionButton: {
    marginHorizontal: 2.5,
    marginVertical: 8,
    height: 40,
    width: 40,
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 5,
  },

});


class Button extends Component {
  render() {
    return (
        <TouchableOpacity onPress={() => this.props.onPress()}>
            <View style={styles.functionButton}>
                {this.props.children}
            </View>
        </TouchableOpacity>
    );
  }
}

export default Button;
