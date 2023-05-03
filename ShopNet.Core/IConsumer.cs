using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopNet.Core
{
    /// <summary>
    /// 启动模式案例
    /// </summary>
    public interface IConsumer : IScopeDependency
    {
        void Receive();

        void Stop();
    }
}