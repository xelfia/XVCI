# XVCI
* X.VCI: A bit experimental runtime for VCI
  * As toolkits (projects), `VCI` and `XVCI` are available on `Unity`
  * XVCI tested with `Unity 2018.3.6f1`

# What is XVCI
* Note that almost VCI features are not implemented on this.
* This is only implemented based on official and/or third-party VCI documents.

> * VirtualCast is a practical VCI enviromnent I know.
> * I have no VR devices currently `2019-03-01`, although VitualCast requires it.
> * So I have no experiences about VirtualCast and actual VCI behaviors.

## Trial on Editor

1. Open `Runtime Loader` scene
2. Select `Test Item original` on `Hierarchy` window
3. Export it to `VCI` file via menu `VRM` / `UniVCI-0.15` / `Export VCI`
4. Play the scene
5. Press `Open` button on `Game View`
6. Select the file (of 2)
7. You can see logs on `Console` window

|from: LUA script line|to: Console output|
|--|--|
|```print(vci.assets);```|VCI.VCIXAssets|
|```print(vci.assets.GetSubItem("Sub1").name);```|Sub1|

8. Select `Sub1` inside `_root_` on `Hierarchy` window
9. Click context menu `⚙` of `VCIX Runtime Sub Item` on `Inspector` window
10. Select `Grab` to test `onGrab` event
11. You can see logs on `Console` window

|from: LUA script line|to: Console output|
|--|--|
|```print("Grab : "..GrabCount)```|Grab : 1|
|```print(target)```|Sub1|

## Licenses / Dependencies

All of these are included in XVCI project.
* VCI v0.15: https://github.com/virtual-cast/VCI
* UniVRM: https://github.com/dwango/UniVRM
* MoonSharp 2.0.0.0: https://github.com/xanathar/moonsharp/

# References
* Official VirtualCast Wiki : https://virtualcast.jp/wiki/doku.php
* MonoSharp : http://www.moonsharp.org/

# Code Examples
* https://qiita.com/sakano/items/adc88174a7dfd32ae248
