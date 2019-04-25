import React, {Component} from 'react';
import {
  AppRegistry,
  StyleSheet,
  Text,
  View,
  Alert,
  Platform,
} from 'react-native';
import RNSketchCanvas from '@terrylinla/react-native-sketch-canvas';


export default class App extends Component {
  render() {
    return (
      <View style={styles.container}>
        <View style={{ flex: 1, flexDirection: 'row' }}>
          <RNSketchCanvas />
        </View>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#F5FCFF',
  },
});