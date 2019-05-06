import React, {Component} from 'react';
import {
  AppRegistry,
  StyleSheet,
  Text,
  View,
  Alert,
  Platform,
  Button,
} from 'react-native';
import RNSketchCanvas from '@terrylinla/react-native-sketch-canvas';
import ImagePicker from 'react-native-image-picker';


const styles = StyleSheet.create({
  container: {
    flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#F5FCFF',
  },
});

export default class App extends Component {
  state = {
    image: null,
  }
  loadImg() {
    // More info on all the options is below in the API Reference... just some common use cases shown here
    const options = {
      title: 'Select Avatar',
      customButtons: [{ name: 'fb', title: 'Choose Photo from Facebook' }],
      storageOptions: {
        skipBackup: true,
        path: 'images',
      },
    };

    /**
     * The first arg is the options object for customization (it can also be null or omitted for default options),
     * The second arg is the callback which sends object: response (more info in the API Reference)
     */
    ImagePicker.showImagePicker(options, (response) => {
      console.log('Response = ', response);

      if (response.didCancel) {
        console.log('User cancelled image picker');
      } else if (response.error) {
        console.log('ImagePicker Error: ', response.error);
      } else if (response.customButton) {
        console.log('User tapped custom button: ', response.customButton);
      } else {
        const source = { uri: response.uri };

        // You can also display the image using data:
        // const source = { uri: 'data:image/jpeg;base64,' + response.data };

        this.setState({
          image: source,
        });
      }
    });
  }

  render() {
    return (
      <View style={styles.container}>
        <Button title="load img" onPress={() => this.loadImg()} />
        <Button title="load img" onPress={() => this.loadImg()} />
        <View style={{ flex: 1, flexDirection: 'row' }}>
          <RNSketchCanvas image={this.state.image} />
        </View>
      </View>
    );
  }
}

