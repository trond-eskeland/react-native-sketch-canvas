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
    UIScrollView *_scrollView;
    RNSketchCanvas * _canvas;
}



- (instancetype)initWithEventDispatcher:(RCTEventDispatcher *)eventDispatcher
{
    self = [super init];
    if (self) {
        _eventDispatcher = eventDispatcher;
        _canvas = [[RNSketchCanvas alloc] initWithEventDispatcher: eventDispatcher ];

        
        self.backgroundColor = [UIColor redColor];
        self.clearsContextBeforeDrawing = YES;
        
//        _scrollView = [[UIScrollView alloc] initWithFrame: self.bounds];
//        _imageView = [[UIImageView alloc] initWithFrame: CGRectMake(0, 0, 2000, 2000)];
//        _imageView.contentMode =  UIViewContentModeScaleToFill; //UIViewContentModeCenter;
//
//
//        _scrollView.contentSize = self.frame.size;
//        [_scrollView addSubview:_imageView];
//        _scrollView.minimumZoomScale = 0.1;
//         _scrollView.maximumZoomScale = 2.0;
//         _scrollView.zoomScale = 2.0;
//        [self addSubview:_scrollView];
//        _scrollView.delegate = self;
        [self addSubview:_canvas];	
        
    }
    return self;
}

- (void)layoutSubviews {
    [super layoutSubviews];
    // [_canvas layoutSubviews];
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
    [_canvas newPath:pathId strokeColor:strokeColor strokeWidth:strokeWidth];
}

- (void) addPath:(int) pathId strokeColor:(UIColor*) strokeColor strokeWidth:(int) strokeWidth points:(NSArray*) points {
    [_canvas addPath:pathId strokeColor:strokeColor strokeWidth:strokeWidth points:points];
}

- (void)deletePath:(int) pathId {
    [_canvas deletePath:pathId];
}

- (void)addPointX: (float)x Y: (float)y {
    [_canvas addPointX:x Y:y];
}

- (void)endPath {
    [_canvas endPath];
}

- (void) clear {
    [_canvas clear];
}

- (void)lockViewPort: (BOOL)enabled {
    NSLog(@"Fix me");
}



// end interface





@end
