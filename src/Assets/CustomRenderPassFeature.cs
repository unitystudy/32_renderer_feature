using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private const string Tag = "CustomRenderPass";
        private RenderTargetIdentifier currentTarget;
        private List<ShaderTagId> shaderTagIdList = new List<ShaderTagId>();
        private FilteringSettings filteringSettings;

        public CustomRenderPass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            shaderTagIdList.Add(new ShaderTagId("PassEdge"));
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, 1 << LayerMask.NameToLayer("Edge Object"));
        }

        // このメソッドはレンダーパスを実行する前に呼び出されます。
        // レンダーターゲットとそのクリア状態を設定するために、また、一時的なレンダーターゲットテクスチャを作成するために使用できます。
        // 空の場合、このレンダーパスはアクティブなカメラのレンダーターゲットにレンダリングします。
        // CommandBuffer.SetRenderTargetは決して呼び出してはいけません。 代わりに<c> ConfigureTarget</c>と<c> ConfigureClear</c>を呼び出します。
        // レンダー パイプラインにより、ターゲットのセットアップとクリアがパフォーマンスの良い方法で行われるようになります。
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
        }

        // ここではレンダリングロジックを実装します。
        // <c>ScriptableRenderContext</c>を使って描画コマンドを発行したり、コマンドバッファを実行したりします。
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // ScriptableRenderContext.submitを呼び出す必要はなく、レンダーパイプラインがパイプライン内の特定のポイントで呼び出します。
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(Tag);
            using (new ProfilingSample(cmd, Tag))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                var cam = renderingData.cameraData.camera;
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortFlags);

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// このレンダー パスの実行中に作成された割り当てられたリソースをすべてクリーンアップします。
        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // レンダーパスを注入する場所を設定します。
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // ここでは、レンダラーに1つまたは複数のレンダーパスを注入することができます。
    // このメソッドは、カメラごとに1回レンダラを設定するときに呼び出されます。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


