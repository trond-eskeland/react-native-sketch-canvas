#bin/bash


path="example/node_modules/@terrylinla/react-native-sketch-canvas"
rm -rf $path
mkdir -p $path


cp -r src "$path/src"
cp -r windows "$path/windows"
cp -r android "$path/android"
cp -r ios "$path/ios"
cp -r resources "$path/resources"

cp index.d.ts "$path/index.d.ts"
cp index.js "$path/index.js"
cp package.json "$path/package.json"
cp RNSketchCanvas.podspec "$path/RNSketchCanvas.podspec"
