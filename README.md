# StarResonanseMiningHelper
自用星痕共鸣挂机挖矿软件

#### 说明
这只是一个极其简单的发送按键的软件，没有内存读取、没有网卡抓包、没有图像识别。

为什么要写一个这种垃圾小软件？明明已经很多自动按F的挂机挖矿软件了，或者用按键精灵也能实现。

因为那些软件都是要把计算机时间完全投入到挂机中，而我的需求是在上班的时候用副屏挂着挖矿。

我可以接受挖矿效率低一点，但除了按下F的那一瞬间，我的电脑大部分时间都还要给我用来工作。

#### 程序功能
1. 自动查找星痕共鸣程序窗口（全屏/窗口化都可以）
2. 将焦点切换到星痕共鸣上
3. 发送鼠标滚轮消息，发送F按键消息
4. 将焦点归还到切换过来之前的程序上
5. 将鼠标归还到切换过来之前的位置上

以上操作会在50毫秒内完成，一轮挖矿循环的默认等待时间为40秒（因为矿石刷新时间30-36秒不固定），即每40秒只会占用50毫秒的时间，几乎不会影响主屏的工作

#### 如何使用
到 [Release](https://github.com/Alex1911-Jiang/StarResonanseMiningHelper/releases) 中下载 StarResonanseMiningHelper.zip 解压，并使用管理员身份运行

#### 如果你想自己编译
由于使用了AOT编译，你需要将 [SkiaSharp](https://github.com/2ndlab/SkiaSharp.Static) 的静态库（libHarfBuzzSharp.lib、libSkiaSharp.lib）放到项目目录，再执行 dotnet publish -c Release -r win-x64

#### 界面截图
<img width="903" height="527" alt="image" src="https://github.com/user-attachments/assets/56ffa489-654e-4511-bb34-b4d90f41163d" />
