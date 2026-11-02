namespace KeanOkta;

public class Settings
{
	public HotkeySpec Hotkey = HotkeySpec.Default();

	public int FloatW = 168;

	public int FloatH = 72;

	public int FloatX = -1;

	public int FloatY = -1;

	// 录制到的用户按键间隔（毫秒），最多 5 个；热键逐位输入时从中随机取一个再 +1~15ms 抖动
	public int[] DelaySamples = new int[0];

	// Okta 组织域名（仅用于“编辑”时打开学校官网入口；换取种子的域名来自二维码令牌本身）
	public string OrgDomain = "sso.kean.edu";
}
