using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net.Sockets;

namespace Pivot.Update
{
    public static class API
    {
        private static InterProcess m_InterProcess;

        static API()
        {
            if (!Deploy.IsDeployed)
                Deploy.PerformDeploy();

            try
            {
                TcpClientChannel ipc = new TcpClientChannel();
                ChannelServices.RegisterChannel(ipc, true);
                RemotingConfiguration.RegisterWellKnownClientType(typeof(InterProcess), "tcp://127.0.0.1:38088/interprocess");
                m_InterProcess = new InterProcess();
            }
            catch (SocketException)
            {
                // If the server isn't running, we won't be able to connect to it.
            }
        }

        /// <summary>
        /// Register an update URI with the current working directory.
        /// </summary>
        /// <param name="uri">The update URI to source updates from.</param>
        public static bool Register(Uri uri)
        {
            try
            {
                if (m_InterProcess != null)
                    return m_InterProcess.Register(uri.ToString(), Environment.CurrentDirectory);
                else
                    throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
            catch (SocketException)
            {
                // If the server isn't running, we won't be able to connect to it.
                throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
        }

        /// <summary>
        /// Returns whether there are updates currently available.
        /// </summary>
        public static bool HasUpdates()
        {
            try
            {
                if (m_InterProcess != null)
                    return m_InterProcess.HasUpdates(Environment.CurrentDirectory);
                else
                    throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
            catch (SocketException)
            {
                // If the server isn't running, we won't be able to connect to it.
                throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
        }

        /// <summary>
        /// Requests that an update be scheduled after the specified number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait before starting the update.</param>
        public static bool ScheduleUpdate(int seconds)
        {
            try
            {
                if (m_InterProcess != null)
                    return m_InterProcess.ScheduleUpdate(Environment.CurrentDirectory, seconds);
                else
                    throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
            catch (SocketException)
            {
                // If the server isn't running, we won't be able to connect to it.
                throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
        }

        /// <summary>
        /// Requests that an update be scheduled after the specified number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait before starting the update.</param>
        /// <param name="restartPath">The path to the executable to start when the update completes.</param>
        public static bool ScheduleUpdate(int seconds, string restartPath)
        {
            try
            {
                if (m_InterProcess != null)
                    return m_InterProcess.ScheduleUpdate(Environment.CurrentDirectory, seconds, restartPath);
                else
                    throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
            catch (SocketException)
            {
                // If the server isn't running, we won't be able to connect to it.
                throw new InvalidOperationException("The Pivot service is not available and thus API calls can not be made.");
            }
        }
    }
}
