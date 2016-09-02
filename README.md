# 功能
- **解析vcxproj**

>    要求vs2010+

- **获取头文件**

- **获取源代码文件**

- **PS**

>    如果vcxproj含有子工程，会递归遍历子工程。

- **解析实例**

>    下面实例是把用cmake组织的llvm源码转换成vs工程。然后解析llvm的vs工程。

![](http://i.imgur.com/XhlrMVK.png)

>    结果如下

![](http://i.imgur.com/czLRaY5.png)