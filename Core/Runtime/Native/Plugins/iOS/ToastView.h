#import <UIKit/UIKit.h>

@interface ToastView : UIView

@property (strong, nonatomic) NSString *text;

+ (void)makeToast: (UIView *)parentView withText:(NSString *)text withDuaration:(float)duration;

@end
