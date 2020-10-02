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
    RNSketchCanvas * _canvas;
    
}
    BOOL _drawEnabled = NO;


- (instancetype)initWithEventDispatcher:(RCTEventDispatcher *)eventDispatcher
{
    self = [super init];
    if (self) {
        _eventDispatcher = eventDispatcher;
        _canvas = [[RNSketchCanvas alloc] initWithEventDispatcher: eventDispatcher ];
        
        self.clearsContextBeforeDrawing = YES;
        self.contentSize = self.frame.size;
        self.minimumZoomScale = 0.5;
        self.maximumZoomScale = 5.001;
        
        self.delegate = self;
        [self addSubview:_canvas];
        
        if (@available(iOS 13.0, *)) {
            self.automaticallyAdjustsScrollIndicatorInsets = NO;
        } else {
            // Fallback on earlier versions
            // Not supported atm...
        }
        
    }
    return self;
}

- (void)dealloc {
    _eventDispatcher = nil;
    _canvas = nil;
}

- (void)layoutSubviews {
    [super layoutSubviews];
    // [_canvas layoutSubviews];
}

// zoom and scroll

- (UIView*)viewForZoomingInScrollView:(UIScrollView *)aScrollView {
    
    return _canvas;
}

- (void)scrollViewDidEndZooming:(UIScrollView *)scrollView withView:(UIView *)view atScale:(CGFloat)scale {
    [self dispatchZoomOffset];
}

- (void)scrollViewDidEndDecelerating:(UIScrollView *)scrollView {
    [self dispatchZoomOffset];
}

- (void)scrollViewDidEndDragging:(UIScrollView *)scrollView willDecelerate:(BOOL)decelerate {
    if (!decelerate) {
        [self dispatchZoomOffset];
    }
}

- (void)dispatchZoomOffset {
    CGFloat zoomScale = self.zoomScale;
    CGFloat offsetVertical = self.contentOffset.y;
    CGFloat offsetHorizontal = self.contentOffset.x;
    /**/
    self->_onChange(@{ @"zoomOffset":  @{
                               @"zoomFactor": @(zoomScale),
                                @"screenImageRatioWidth": @(1),
                                @"screenImageRatioHeight": @(1),
                                @"horizontalOffset": @(offsetHorizontal + 5),
                                @"verticalOffset":  @(offsetVertical + 5)
    } });
    
    NSLog(@"scrollEnded: zoomScale: %f, offsetVertical: %f, offsetHorizontal: %f", zoomScale, offsetVertical, offsetHorizontal);
}


// interface


- (BOOL)openSketchFile:(NSString *)filename directory:(NSString*) directory contentMode:(NSString*)mode {
    _drawEnabled = NO;
    BOOL result = [_canvas openSketchFile:filename directory:directory contentMode:mode];
    self.zoomScale = 1.0001;
    // [self dispatchZoomOffset];
    return result;
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
    [_canvas addPointX:x / self.zoomScale Y:y / self.zoomScale];
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

        // self->_canvas = nil;
        NSLog(@"File saved");
    }];


}
- (NSString*) transferToBase64OfType: (NSString*) type withTransparentBackground: (BOOL) transparent includeImage:(BOOL)includeImage includeText:(BOOL)includeText cropToImageSize:(BOOL)cropToImageSize {
   return [_canvas transferToBase64OfType:type withTransparentBackground:  transparent includeImage:includeImage includeText:includeText cropToImageSize:cropToImageSize];
}


// end interface





@end
