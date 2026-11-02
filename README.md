# Kean Okta Verify · TOTP

> 黑金风格的 Okta TOTP 桌面验证码小工具 —— 屏幕扫码自动配置、真人节奏热键录入、置顶悬浮窗。绿色免安装，仅依赖系统自带的 .NET Framework 4.8。

为频繁登录学校官网（Okta SSO）的同学设计：把 TOTP 验证码放到桌面，登录时按一下热键逐位键入，不必再掏手机打开 Okta Verify。

允许二次开发，请在UI页面署名原作者  方木金 fangmujin
---

## 功能特性

| 功能 | 说明 |
| --- | --- |
| **一键扫码配置** | 点“编辑”后弹出置顶引导窗，按 6 步在 Okta 后台添加另一个 TOTP 验证方式；程序自动截屏识别页面二维码，联网换取种子，成功后直接开始计算 |
| **引导小窗** | 白色 60% 透明、置顶、可任意拖动；每步点“完成”跳下一步；识别失败可“框选识别”或回退“手动粘贴” |
| **真人节奏热键录入** | 热键逐位键入验证码（SendInput 真实数字键 VK + 扫描码）；先用“延迟模拟”录下你自己的 5 次按键间隔，之后每次录入按你的节奏 + 1~15ms 随机抖动，降低被“脚本防注入”策略拦截的概率 |
| **三组滚动动画** | 上一组 / 当前组 / 下一组 30 秒滚动动画，毫秒级倒计时，完整保留经典 Okta Verify 观感 |
| **公式区** | 实时展示本地时间、Unix 时间、计数器 T、HMAC-SHA1 动态截断全过程（0.3~0.8 秒随机刷新，仅主界面可见时运行） |
| **最小悬浮窗** | 置顶小窗可拖动；“悬浮窗编辑”面板可实时拖动宽/高（像素），验证码数字随高度等比例缩放 |
| **黑金 UI** | 深黑底 + 金色验证码 + Consolas 等宽字体，主界面 / 悬浮窗 / 托盘 / 全部对话框统一风格 |
| **域名可配置** | 默认 `sso.kean.edu`，可在引导窗改成任何 Okta 组织域名（如 `xxx.okta.com`），其他学校的同学也能用 |
| **安全存储** | 种子用 Windows DPAPI（CurrentUser）加密存于程序目录，仅本机当前用户可解密；不经过任何第三方服务器 |

## 快速开始

### 下载即用（推荐）

从 [Releases](../../releases) 下载最新版 zip，解压后双击 `KeanOktaTOTP.exe`：

- 依赖系统自带的 .NET Framework 4.8（Windows 10 1809+ / Windows 11 均已内置，无需安装）；
- 绿色免安装，整个文件夹拷走即用；
- 首次运行自动生成 `settings.ini`；配置种子后生成 `seed.dat`（DPAPI 加密）。

### 从源码构建

```bash
git clone https://github.com/<your-name>/KeanOktaTOTP.git
cd KeanOktaTOTP/src
dotnet build -c Release
```

产物在 `bin/Release/net48/`（需要 .NET SDK；项目通过 `Microsoft.NETFramework.ReferenceAssemblies`
在没有 4.8 开发者包的机器上也能编译，运行时仍依赖系统 .NET Framework 4.8）。

二维码识别依赖 [ZXing.Net](https://github.com/micjahn/ZXing.Net)（NuGet 0.16.9），构建时自动还原。

## 首次配置（带引导，约 1 分钟）

1. 运行程序，点主界面种子区右侧的 **“编辑”**；
2. 程序自动打开学校 Okta 官网，并弹出置顶引导窗（白色 60% 透明、可拖动）；
3. 按引导操作，每点一次“完成”跳下一步：

   ```
   ① 自行登录官网
   ② 使用旧的手机版 Okta 验证
   ③ 右上角「账户设置」
   ④ 安全方法 →「设置另一个」→ 选 Okta Verify
   ⑤ 再次使用手机进行安全验证
   ⑥ 点“识别二维码”
   ```

4. 程序自动截屏识别二维码 → 联网向 Okta 换取种子 → **引导窗关闭，直接弹回程序开始计算**。

识别失败时的兜底：引导窗内可**重试**、**框选识别**（鼠标圈出二维码）、或**手动粘贴**
`oktaverify://` 令牌 / Base32 种子。

## 日常使用

- **托盘**：悬停显示当前 6 位码；左键弹实时小窗；右键菜单（主界面 / 悬浮窗 / 开机自启 / 退出）。
- **全局热键**：默认 `Ctrl + 左Shift + “+/=”`，在任意输入框按下即逐位键入当前验证码（不按回车）。
  - 自定义热键：主界面“自定义热键”，录制时自动检测组合键是否被系统占用。
  - 延迟模拟：连按 6 个数字键录下你的按键节奏，热键录入从此“像真人”。
- **主界面**：三组验证码 + 公式区 + 六个功能按钮（自定义热键 / 开机自启 / 最小悬浮窗 / 延迟模拟 / 悬浮窗编辑 / 放置到托盘）。
- **悬浮窗**：双击文字改尺寸；“悬浮窗编辑”实时调宽高，数字等比缩放。

## 安全与隐私

- 种子等同于第二把登录钥匙，**切勿外发、截图或上传网盘**；
- `seed.dat` 用 DPAPI（CurrentUser）加密，只对“本机当前 Windows 用户”可解密；换电脑 / 换用户需重新配置；
- 本工具不经过任何第三方服务器；换取种子仅与 Okta 组织域名通信；
- 本仓库**不含任何种子、账号或个人数据**；向 Okta 注册的设备名带时间戳（如 `TOTP1102`），方便在后台识别和注销。

## 已知限制

- 无法向“以管理员身份运行”的窗口自动键入（Windows UIPI 权限隔离）——系统级安全边界，请手动抄码；
- 个别网页 / 安全控件禁止一切合成输入，只能手动抄码；
- 多显示器 + 混合 DPI 环境下，全屏识别以主显示器为准，其他屏幕上的二维码请用“框选识别”；
- 若组织强制 Okta Push 或数字挑战（number challenge），纯 TOTP 验证码不能替代。

## 项目结构

```
src/
├── KeanOktaTOTP.csproj      # net48 / WinForms / ZXing.Net
├── app.ico
└── KeanOkta/
    ├── Program.cs           # 入口（--tray 直接进托盘）
    ├── AppContext.cs        # 应用上下文：托盘、热键、各窗体调度
    ├── MainForm.cs          # 主界面：三组验证码 + 公式区 + 功能按钮
    ├── CodesPanel.cs        # 30 秒换组滚动动画
    ├── Totp.cs              # TOTP：HMAC-SHA1 / 30s / 6 位 / 动态截断
    ├── Enroll.cs            # oktaverify:// 令牌 → Okta 换取 sharedSecret
    ├── SeedSaver.cs         # 种子保存统一入口（令牌 / otpauth / Base32）
    ├── QrScanner.cs         # 截屏 + ZXing 解码（自适应 DPI 缩放倍率）
    ├── QrRegionForm.cs      # 框选识别全屏遮罩
    ├── QrPickDialog.cs      # 多二维码选择
    ├── GuideForm.cs         # 置顶引导窗（白 60% 透明、可拖动）
    ├── AutoType.cs          # 热键逐位键入（真实 VK + 节奏抖动）
    ├── DelayRecorder.cs     # 录制真实按键间隔（WH_KEYBOARD_LL）
    ├── KeyboardHook.cs      # 全局热键
    ├── FloatForm.cs         # 最小悬浮窗
    ├── FloatEditForm.cs     # 悬浮窗实时尺寸编辑
    ├── Store.cs / Settings.cs  # 设置与种子存取（DPAPI）
    ├── Theme.cs             # 黑金主题
    └── ...                  # HotkeyDialog / SeedDialog / PopupForm / InputDialog / AutoStart 等
```

## 技术要点

- **TOTP**：HMAC-SHA1，counter = UnixTime / 30，动态截断（`H[19] & 0x0F` 取偏移），`bin mod 10^6`；
- **种子获取**：二维码内容为 `oktaverify://?t=…&f=…&s=…` 一次性令牌，程序取 `s`（Okta 组织域名）
  调 `/oauth2/v1/keys` 与 `/idp/authenticators` 换取 `sharedSecret`；
- **DPI 适配**：程序保持 DPI-unaware（UI 缩放与系统习惯一致），截屏时用 `EnumDisplaySettings`
  取物理分辨率换算倍率，保证 125% / 150% 等缩放下截到完整物理屏幕；
- **防注入对抗**：真实数字键 VK + 硬件扫描码 + 用户录制的按键节奏 + 1~15ms 随机抖动。

## License

[MIT](LICENSE) © 2026 KeanOktaTOTP contributors
