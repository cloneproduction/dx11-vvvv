﻿using FeralTic.DX11;
using FeralTic.DX11.Resources;
using FlareTic.API;
using FlareTic.API.DX11;
using FlareTic.DX11.Interfaces;
using FlareTic.DX11.SceneGraph;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlareTic.Nodes.Kinect2
{
    [SceneGraphNode(Name = "KinectColor", Category = "Devices", SystemName = "flt.sg.kinectcolor", ShowInList = true)]
    public class KinectColorTexture : ISceneGraphNodeInstance, IResourceObject
    {
        private ISceneGraphNodeContainer container;

        private DX11Texture2D texture;
        private RenderDevice device;

        private IKinectManager manager;

        bool invalidate;

        public void AssignContainer(ISceneGraphNodeContainer container)
        {
            this.container = container;
            this.container.BindInterface<IResourceObject>(this);
            this.container.AddHandler<IKinectManager>(false);
            this.container.Sink.SubObjectConnected += Sink_SubObjectConnected;
            this.container.Sink.SubObjectDisconnected += Sink_SubObjectDisconnected;
            this.device = container.RenderDevice;
        }

        void Sink_SubObjectDisconnected(object instance)
        {
            manager.NewColorFrame -= manager_NewColorFrame;
            manager = null;
            if (this.texture != null)
            {
                this.texture.Dispose();
                this.texture = null;
            }
        }

        void manager_NewColorFrame(object sender, EventArgs e)
        {
            invalidate = true;
        }

        void Sink_SubObjectConnected(object instance)
        {
            manager = (IKinectManager)instance;
            manager.NewColorFrame += manager_NewColorFrame;
        }

        public void Dispose()
        {

        }

        public FeralTic.IDxShaderResource Resource
        {
            get
            {
                return this.texture;
            }
        }

        public void Update(IRenderObject parent, SceneGraphSettings settings)
        {
            if (this.manager != null)
            {
                if (this.invalidate)
                {
                    if (this.texture == null)
                    {
                        this.texture = DX11Texture2D.CreateDynamic(this.device,1920,1080,SharpDX.DXGI.Format.B8G8R8A8_UNorm);
                    }

                    this.texture.WriteData(settings.RenderContext, this.manager.ColorFrame, 1920*1080*4, 4);

                    this.invalidate = false;
                }
            }
        }
    }
}
