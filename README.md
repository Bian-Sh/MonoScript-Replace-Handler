# MonoScript Replace Handler

> 本说明有配图，请确保你的网络畅通，图文模式下阅读更佳。

<a id="chinese">[English](#english)</a>

## 前言

**当你**看到某个插件一个 UI 页面功能就是你将要搭建的，你又不可能直接拿来用（请注意，直接拿来用下次插件更新时极有可能覆盖你的修改），你可以直接 Ctrl+D 复刻一份这样的 Prefab Asset，同样的道理，脚本也必须 Ctrl+D 一份，但此时问题来了：怎么把脚本替换了还能保证之前的引用还在呢？

本工具就是为了解决上述问题、复用 Prefab Asset 而作！

![](Packages/MonoScript%20Replace%20Handler/Doc~/mrh.gif)

## 实现

核心思路是直接替换 MonoScript 实例即可切换脚本且保留所有数据，这也是本工具名称的由来。

```csharp
    private void ConvertMonoScript(SerializedObject so_Target, MonoScript source)
    {
        so_Target.UpdateIfRequiredOrScript();
        so_Target.FindProperty("m_Script").objectReferenceValue = source;
        so_Target.ApplyModifiedProperties();
    }
```

## 使用：

1. Clone 本项目，将 ``Packages `` 文件夹中``MonoScript Replace Handler``拖放到你的项目即可
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/location.png)

2. 或者你可以通过 UPM 安装：点击 Unity Package Manager 左上角 “+” 号，然后点击图示菜单，在接下来的输入框中输入``https://github.com/Bian-Sh/MonoScript-Replace-Handler.git/?path=Packages/MonoScript Replace Handler``` 即可（大小写敏感、 国区可能失败哦！）)
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/install.png)

3. 选中 Prefab Assets，对着需要替换组件的 Inspector Header 点击右键，点击 Replace ，接下来的操作会在弹窗中进行。（请注意 Transform 不支持被替换）
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/contextmenu.png)

4. 在图示弹窗中按步骤选择用于替换的脚本，点击确定即可，（请注意没有继承 Behaviour 的脚本不支持作为替换的脚本）
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/window.png)

5. 支持 Ctrl+z 撤销脚本替换操作，但无论何时你都不允许使用 Ctrl+ Y 重做这个脚本替换动作，如果你满身反骨，恭喜你编辑器将当即奔溃！

6. 由于我们的操作是一种非常规的，所以尽量克制使用，不要期望它的替换行为是万能的。

## Reference

* [[Unity3D] 子类组件怎么无损替换父组件 - 简书](https://www.jianshu.com/p/baf1a0eb0298)

* https://forum.unity.com/threads/disconnecting-is-no-longer-implemented.656377/

* 这个插件做来好玩的，其实 Debug 模式就可以方便的替换 Component 哦！

![image](https://github.com/user-attachments/assets/02750119-1aef-4a91-8fd0-214897f84034)

## 贡献指南

如果你有任何问题或建议，欢迎提交 issue 或 pull request。

## License

遵循 MIT 开源协议

<a id="english">[ Top ↑ ](#chinese)</a>

> This document includes images, please ensure your network connection is stable for an optimal reading experience in image-text mode.

## Foreword

**When you** see a plugin where a single UI page functionality is exactly what you need to build, and you can't simply use it directly (please note, using it directly might lead to your modifications being overwritten during the next plugin update), you can Ctrl+D to clone a copy of such a Prefab Asset. Similarly, you must also Ctrl+D a copy of the script, but then the question arises: how can you replace the script and still ensure that the previous references remain intact?

This tool is designed to solve the above problem and facilitate the reuse of Prefab Assets!

![](Packages/MonoScript%20Replace%20Handler/Doc~/mrh.gif)

## Implementation

The core idea is to directly replace the MonoScript instance to switch scripts while retaining all data, which is the origin of this tool's name.

```csharp
    private void ConvertMonoScript(SerializedObject so_Target, MonoScript source)
    {
        so_Target.UpdateIfRequiredOrScript();
        so_Target.FindProperty("m_Script").objectReferenceValue = source;
        so_Target.ApplyModifiedProperties();
    }
```

## Usage:

1. Clone this project, and drag the ``MonoScript Replace Handler`` folder from ``Packages`` into your project.
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/location.png)

2. Or you can install via UPM: click the “+” icon in the top left corner of Unity Package Manager, then click on the illustrated menu, and enter ``https://github.com/Bian-Sh/MonoScript-Replace-Handler.git/?path=Packages/MonoScript Replace Handler`` in the following input box (case-sensitive).
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/install.png)

3. Select Prefab Assets, right-click on the Inspector Header of the component to be replaced, click Replace, and the following operations will be carried out in the popup window. (Please note that Transform is not supported for replacement.)
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/contextmenu.png)

4. In the illustrated popup window, follow the steps to select the script for replacement and click confirm. (Please note that scripts not inheriting from Behaviour are not supported as replacement scripts.)
   
   ![](Packages/MonoScript%20Replace%20Handler/Doc~/window.png)

5. Ctrl+z is supported to undo the script replacement operation, but at no time are you allowed to use Ctrl+Y to redo this script replacement action. If you do, congratulations, your editor will crash immediately!

6. Since our operation is unconventional, please use it sparingly and do not expect its replacement behavior to be infallible.

## Reference

* [[Unity3D] How to Replace a Parent Component with a Subclass Component Without Loss - 简书](https://www.jianshu.com/p/baf1a0eb0298)

* https://forum.unity.com/threads/disconnecting-is-no-longer-implemented.656377/

* ![image](https://github.com/user-attachments/assets/02750119-1aef-4a91-8fd0-214897f84034)

## Contribution Guide

If you have any questions or suggestions, feel free to submit an issue or pull request.

## License

This project is licensed under the MIT License.
