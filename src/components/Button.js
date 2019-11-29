import React, { Component } from 'react';
import { TouchableOpacity, View, StyleSheet } from 'react-native';

const styles = StyleSheet.create({
  functionButton: {
    marginHorizontal: 2.5,
    marginVertical: 10,
    height: 38,
    width: 38,
    justifyContent: 'center',
    alignItems: 'center',
    borderRadius: 5,
  },

});


// eslint-disable-next-line react/prefer-stateless-function
class Button extends Component {
  render() {
    return (
      <TouchableOpacity
        style={{ backgroundColor: 'white' }}
        onPress={() => this.props.onPress()}
      >
        <View style={styles.functionButton}>
            {this.props.children}
        </View>
      </TouchableOpacity>
    );
  }
}

export default Button;
