namespace TBS.Contracts.Events
{
    /// <summary>
    /// 游戏流程相关事件
    /// </summary>

    /// <summary>
    /// 游戏启动请求事件 - 由 MainMenuPresenter 发布，GameManager 接收
    /// </summary>
    public struct GameStartRequestedEvent { }

    /// <summary>
    /// 游戏退出请求事件 - 由 UI 发布，GameManager 接收
    /// </summary>
    public struct GameExitRequestedEvent { }
}
