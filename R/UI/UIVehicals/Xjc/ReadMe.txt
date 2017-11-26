①XjcBody{Image,Height=56,Width=49},Top=88;
②XjcMark{TextBox,Height=36,Width=36};
③WaterCan{TextBox,Height=49,Width=49},Top=92;
④Shelf(干熄焦焦罐的底座){Image,Height=49,Width=49},Top=92;
⑤Can(干熄焦的焦罐){Ellipse,Height=42,Width=42}
D-->Dry   开头的UserControl 代表的是电机车的意思，即为干熄焦；
W-->Water 开头的UserControl 代表的是熄焦车的意思，即为水熄焦；
can  焦罐
headstock 车头
mark TextBox  用来显示车辆的炉号；
车体 焦罐(水熄、干熄) 的宽度Width=49 ，煤塔、端台的快递=84，炭化室宽度=7，由此来推算得到两辆熄焦车的Margin.Left
左侧的熄焦车均为1#熄焦车，右侧的为2#熄焦车
