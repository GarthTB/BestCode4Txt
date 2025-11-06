# BestCode4Txt | 最优编码查找器 🧮

`BestCode4Txt` 是一个基于字典树和动态规划的命令行工具，用于在给定
输入法编码方案下，为任意中文文本计算总按键开销最小的完整编码路径。
适用于拼音、五笔、仓颉、郑码等输入法的编码优化分析。

[![框架 .NET 10.0](https://img.shields.io/badge/框架-.NET%20%2010.0-blueviolet)](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)
[![语言 C# 14.0](https://img.shields.io/badge/语言-C%23%2014.0-navy.svg)](https://github.com/dotnet/csharplang)
[![许可 MIT](https://img.shields.io/badge/许可-MIT-brown)](https://mit-license.org)
[![平台 Windows x64](https://img.shields.io/badge/平台-Windows%20x64-yellow.svg)](https://github.com/GarthTB/BestCode4Txt)
[![版本 1.0.0](https://img.shields.io/badge/版本-1.0.0-brightgreen)](https://github.com/GarthTB/BestCode4Txt/releases/latest)
[![说明 README.md](https://img.shields.io/badge/说明-README.md-red)](https://github.com/GarthTB/BestCode4Txt/blob/master/README.md)

## ✨ 特性

- 🚀 **高效**：时间复杂度接近线性，1秒可计算100万字
- 📊 **全能**：附带编码分析，直接获取码长等30+指标
- ⚙ **自动**：通过TOML文件配置所有参数，无交互
- 📦 **易用**：AOT编译为原生`win-x64`可执行文件，解压即用

## 📥 安装与使用

### 系统要求

- 操作系统：Windows 10 或更高版本
- 架构：x64

> **注意：AOT编译，无需安装.NET运行时**

### 使用步骤

1. 下载 [最新版本包](https://github.com/GarthTB/BestCode4Txt/releases/latest) 并解压
2. 按需修改目录下的 `Config.toml`
3. 运行程序 `BestCode4Txt.exe`
    - 推荐方式：在控制台中运行，以查看输出日志
    - 简便方式：直接运行，执行完毕后自动退出

## 📋 配置文件

程序的所有行为均由 `Config.toml` 文件控制。
以下是一个示例配置及详细说明（随包附带）：

``` toml
# 示例配置：按需修改，然后保存为程序目录下的`Config.toml`，作为输入参数

# 待编码的文本输入路径
input_path = "Input/TestText.txt"

# 编码及其开销的连接策略：
# SpaceOrPunct - 空格或标点
# NoGap - 无间隔
# Jd6 - 键道6顶功
link_strat = "SpaceOrPunct"

# 最优编码及其分析报告输出路径（存在则覆写）
output_path = "Output/TestTextCodeReport.txt"

# 词库（RIME格式）路径
dict_path = "Cfg/Dict.yaml"

# 键对-开销（当量）路径
costs_path = "Cfg/Costs.tsv"

# 键盘布局配置路径
layout_path = "Cfg/Layout.toml"
```

## 🛠 技术栈

- **框架**：.NET 10.0
- **语言**：C# 14.0
- **依赖**：[Tomlyn](https://github.com/xoofx/Tomlyn)

## 📜 开源信息

- **作者**：GarthTB | 天卜 <g-art-h@outlook.com>
- **许可证**：[MIT 许可证](https://mit-license.org)
    - 可以自由使用、修改和分发软件
    - 可以用于商业项目
    - 必须保留原始版权声明 `Copyright (c) 2025 GarthTB | 天卜`
- **项目地址**：https://github.com/GarthTB/BestCode4Txt

## 📝 更新日志

### v1.1.0 (20251106)

- 迁移至[新仓库](https://github.com/GarthTB/InputEncoderDP)

### v1.0.0 (20251030)

- 首个发布！
