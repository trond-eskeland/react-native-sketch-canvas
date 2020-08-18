//
//  RNSketchViewPort.m
//
//
//  Created by Trond Eskeland on 04/08/2020.
//

#import "RNSketchCanvasManager.h"
#import "RNSketchViewPort.h"
#import "RNSketchData.h"
#import <React/RCTEventDispatcher.h>
#import <React/RCTView.h>
#import <React/UIView+React.h>
#import "Utility.h"

@implementation RNSketchViewPort
{
    RCTEventDispatcher *_eventDispatcher;
    //UIScrollView *_scrollView;
    RNSketchCanvas * _canvas;
    
}
    BOOL _drawEnabled = NO;


- (instancetype)initWithEventDispatcher:(RCTEventDispatcher *)eventDispatcher
{
    self = [super init];
    if (self) {
        _eventDispatcher = eventDispatcher;
        _canvas = [[RNSketchCanvas alloc] initWithEventDispatcher: eventDispatcher ];
        

        
        self.backgroundColor = [UIColor blueColor];
        self.clearsContextBeforeDrawing = YES;
        
//        _scrollView = [[UIScrollView alloc] initWithFrame: self.bounds];
//        _imageView = [[UIImageView alloc] initWithFrame: CGRectMake(0, 0, 2000, 2000)];
//        _imageView.contentMode =  UIViewContentModeScaleToFill; //UIViewContentModeCenter;
//
//
        self.contentSize = self.frame.size;
//        [_scrollView addSubview:_imageView];
        self.minimumZoomScale = 1.0;
         self.maximumZoomScale = 1.001;
        
//        [self addSubview:_scrollView];
        self.delegate = self;
        [self addSubview:_canvas];
        self.zoomScale = 1.001;
        
    }
    return self;
}

- (void)layoutSubviews {
    [super layoutSubviews];
    [_canvas layoutSubviews];
}
// zoom and scroll

- (UIView*)viewForZoomingInScrollView:(UIScrollView *)aScrollView {
    return _canvas;
}


// interface


- (BOOL)openSketchFile:(NSString *)filename directory:(NSString*) directory contentMode:(NSString*)mode {

    return [_canvas openSketchFile:filename directory:directory contentMode:mode];
}

- (void)setCanvasText:(NSArray *)aText {
    [_canvas setCanvasText:aText];
}

- (void)newPath:(int) pathId strokeColor:(UIColor*) strokeColor strokeWidth:(int) strokeWidth {
    if (!_drawEnabled) {
        return;
    }
    [_canvas newPath:pathId strokeColor:strokeColor strokeWidth:strokeWidth];
}

- (void)addPath:(int) pathId strokeColor:(UIColor*) strokeColor strokeWidth:(int) strokeWidth points:(NSArray*) points {
    if (!_drawEnabled) {
        return;
    }
    [_canvas addPath:pathId strokeColor:strokeColor strokeWidth:strokeWidth points:points];
}

- (void)deletePath:(int) pathId {
    [_canvas deletePath:pathId];
}

- (void)addPointX: (float)x Y: (float)y {
    if (!_drawEnabled) {
        return;
    }
    [_canvas addPointX:x Y:y];
}

- (void)endPath {
    if (!_drawEnabled) {
        return;
    }
    [_canvas endPath];
}

- (void)clear {
    [_canvas clear];
}

- (void)lockViewPort: (BOOL)enabled {
    NSLog(@"Fix me");
    if (enabled) {
        self.pinchGestureRecognizer.enabled = YES;
        self.panGestureRecognizer.enabled = YES;
        _drawEnabled = NO;
    } else {
        self.pinchGestureRecognizer.enabled = NO;
        self.panGestureRecognizer.enabled = NO;
        _drawEnabled = YES;
    }
}

- (void)saveImageOfType:(NSString*) type folder:(NSString*) folder filename:(NSString*) filename withTransparentBackground:(BOOL) transparent includeImage:(BOOL)includeImage includeText:(BOOL)includeText cropToImageSize:(BOOL)cropToImageSize {
    [_canvas saveImageOfType:type folder:folder filename:filename withTransparentBackground:transparent includeImage:includeImage includeText:includeText cropToImageSize:cropToImageSize onChange:^(BOOL success, NSURL* fileURL) {
        if (success) {
           self->_onChange(@{ @"success": @YES, @"path": [fileURL path]});
        } else {
           self->_onChange(@{ @"success": @NO, @"path": [NSNull null]});
        }
        
        NSLog(@"File saved");
    }];


}
- (NSString*) transferToBase64OfType: (NSString*) type withTransparentBackground: (BOOL) transparent includeImage:(BOOL)includeImage includeText:(BOOL)includeText cropToImageSize:(BOOL)cropToImageSize {
   return [_canvas transferToBase64OfType:type withTransparentBackground:  transparent includeImage:includeImage includeText:includeText cropToImageSize:cropToImageSize];
}


// end interface





@end
