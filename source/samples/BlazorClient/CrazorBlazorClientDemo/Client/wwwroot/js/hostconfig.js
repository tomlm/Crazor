﻿const adaptiveHostConfig = new AdaptiveCards.HostConfig({
    "choiceSetInputValueSeparator": ",",
    "supportsInteractivity": true,
    "spacing": {
        "small": 8,
        "default": 12,
        "medium": 16,
        "large": 20,
        "extraLarge": 24,
        "padding": 16
    },
    "separator": {
        "lineThickness": 1,
        "lineColor": "#EEEEEE"
    },
    "imageSizes": {
        "small": 32,
        "medium": 52,
        "large": 100
    },
    "fontTypes": {
        "default": {
            "fontFamily": "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif",
            "fontSizes": {
                "small": 12,
                "default": 14,
                "medium": 14,
                "large": 18,
                "extraLarge": 24
            },
            "fontWeights": {
                "lighter": 200,
                "default": 400,
                "bolder": 600
            }
        },
        "monospace": {
            "fontFamily": "'Courier New', Courier, monospace",
            "fontSizes": {
                "small": 12,
                "default": 14,
                "medium": 14,
                "large": 18,
                "extraLarge": 24
            },
            "fontWeights": {
                "lighter": 200,
                "default": 400,
                "bolder": 600
            }
        }
    },
    "textStyles": {
        "heading": {
            "fontType": "default",
            "size": "large",
            "weight": "bolder",
            "color": "default",
            "isSubtle": false
        }
    },
    "textBlock": {
        "headingLevel": 2
    },
    "containerStyles": {
        "default": {
            "foregroundColors": {
                "default": {
                    "default": "#ff252424",
                    "subtle": "#bf252424"
                },
                "dark": {
                    "default": "#252424",
                    "subtle": "#bf252424"
                },
                "light": {
                    "default": "#ffffff",
                    "subtle": "#fff3f2f1"
                },
                "accent": {
                    "default": "#6264a7",
                    "subtle": "#8b8cc7"
                },
                "good": {
                    "default": "#92c353",
                    "subtle": "#e592c353"
                },
                "warning": {
                    "default": "#f8d22a",
                    "subtle": "#e5f8d22a"
                },
                "attention": {
                    "default": "#c4314b",
                    "subtle": "#e5c4314b"
                }
            },
            "borderColor": "#CCCCCC",
            "backgroundColor": "#ffffff"
        },
        "emphasis": {
            "foregroundColors": {
                "default": {
                    "default": "#ff252424",
                    "subtle": "#bf252424"
                },
                "dark": {
                    "default": "#252424",
                    "subtle": "#bf252424"
                },
                "light": {
                    "default": "#ffffff",
                    "subtle": "#fff3f2f1"
                },
                "accent": {
                    "default": "#6264a7",
                    "subtle": "#8b8cc7"
                },
                "good": {
                    "default": "#92c353",
                    "subtle": "#e592c353"
                },
                "warning": {
                    "default": "#f8d22a",
                    "subtle": "#e5f8d22a"
                },
                "attention": {
                    "default": "#c4314b",
                    "subtle": "#e5c4314b"
                }
            },
            "borderColor": "#666666",
            "backgroundColor": "#fff9f8f7"
        },
        "accent": {
            "borderColor": "#62A8F7",
            "backgroundColor": "#C7DEF9",
            "foregroundColors": {
                "default": {
                    "default": "#ff252424",
                    "subtle": "#bf252424"
                },
                "dark": {
                    "default": "#252424",
                    "subtle": "#bf252424"
                },
                "light": {
                    "default": "#ffffff",
                    "subtle": "#fff3f2f1"
                },
                "accent": {
                    "default": "#6264a7",
                    "subtle": "#8b8cc7"
                },
                "good": {
                    "default": "#92c353",
                    "subtle": "#e592c353"
                },
                "warning": {
                    "default": "#f8d22a",
                    "subtle": "#e5f8d22a"
                },
                "attention": {
                    "default": "#c4314b",
                    "subtle": "#e5c4314b"
                }
            }
        },
        "good": {
            "borderColor": "#69E569",
            "backgroundColor": "#CCFFCC",
            "foregroundColors": {
                "default": {
                    "default": "#ff252424",
                    "subtle": "#bf252424"
                },
                "dark": {
                    "default": "#252424",
                    "subtle": "#bf252424"
                },
                "light": {
                    "default": "#ffffff",
                    "subtle": "#fff3f2f1"
                },
                "accent": {
                    "default": "#6264a7",
                    "subtle": "#8b8cc7"
                },
                "good": {
                    "default": "#92c353",
                    "subtle": "#e592c353"
                },
                "warning": {
                    "default": "#f8d22a",
                    "subtle": "#e5f8d22a"
                },
                "attention": {
                    "default": "#c4314b",
                    "subtle": "#e5c4314b"
                }
            }
        },
        "attention": {
            "borderColor": "#FF764C",
            "backgroundColor": "#FFC5B2",
            "foregroundColors": {
                "default": {
                    "default": "#ff252424",
                    "subtle": "#bf252424"
                },
                "dark": {
                    "default": "#252424",
                    "subtle": "#bf252424"
                },
                "light": {
                    "default": "#ffffff",
                    "subtle": "#fff3f2f1"
                },
                "accent": {
                    "default": "#6264a7",
                    "subtle": "#8b8cc7"
                },
                "good": {
                    "default": "#92c353",
                    "subtle": "#e592c353"
                },
                "warning": {
                    "default": "#f8d22a",
                    "subtle": "#e5f8d22a"
                },
                "attention": {
                    "default": "#c4314b",
                    "subtle": "#e5c4314b"
                }
            }
        },
        "warning": {
            "borderColor": "#FFBC51",
            "backgroundColor": "#FFE2B2",
            "foregroundColors": {
                "default": {
                    "default": "#ff252424",
                    "subtle": "#bf252424"
                },
                "dark": {
                    "default": "#252424",
                    "subtle": "#bf252424"
                },
                "light": {
                    "default": "#ffffff",
                    "subtle": "#fff3f2f1"
                },
                "accent": {
                    "default": "#6264a7",
                    "subtle": "#8b8cc7"
                },
                "good": {
                    "default": "#92c353",
                    "subtle": "#e592c353"
                },
                "warning": {
                    "default": "#f8d22a",
                    "subtle": "#e5f8d22a"
                },
                "attention": {
                    "default": "#c4314b",
                    "subtle": "#e5c4314b"
                }
            }
        }
    },
    "actions": {
        "maxActions": 6,
        "spacing": "Default",
        "buttonSpacing": 8,
        "showCard": {
            "actionMode": "Inline",
            "inlineTopMargin": 16,
            "style": "emphasis"
        },
        "preExpandSingleShowCardAction": false,
        "actionsOrientation": "Horizontal",
        "actionAlignment": "left"
    },
    "adaptiveCard": {
        "allowCustomStyle": false
    },
    "imageSet": {
        "imageSize": "Medium",
        "maxImageHeight": 100
    },
    "factSet": {
        "title": {
            "size": "Default",
            "color": "Default",
            "isSubtle": false,
            "weight": "Bolder",
            "warp": true
        },
        "value": {
            "size": "Default",
            "color": "Default",
            "isSubtle": false,
            "weight": "Default",
            "warp": true
        },
        "spacing": 16
    }
});

