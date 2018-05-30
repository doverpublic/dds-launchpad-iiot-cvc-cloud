using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iot.Common
{
    public class SessionManager
    {
        private static string       defaultCookieName = "___Launchpad-Session-Id___";
        private static ConcurrentDictionary<string, SessionContainer>    sessionsBag = new ConcurrentDictionary<string, SessionContainer>();
        private static TimeSpan     sessionMaxIdleTime = new TimeSpan(0,20,0);  // in minutes
        private static bool         sessionThreadFlag = false;
        private static Thread       sessionThread = null;

        private static ParameterContainer singletonParamater = new ParameterContainer();

        public static string GetSingletonParameter()
        {
            string strRet = null;

            lock(singletonParamater)
            {
                strRet = singletonParamater.GetParameter();
            }

            return strRet;
        }

        public static void SetSingletonParameter( string value )
        {
            bool isParameterPresent = true;
            int sleepInterval = 1;

            while(isParameterPresent )
            {
                lock (singletonParamater)
                {
                    isParameterPresent = singletonParamater.IsParameterPresent();

                    if( !isParameterPresent || sleepInterval > 4 )
                    {
                        singletonParamater.SetPameter(value);
                        isParameterPresent = false;
                    }
                }

                if( isParameterPresent )
                {
                    Thread.Sleep(sleepInterval * 1000 );
                    sleepInterval *= 2;
                }
            }
        }

        public static string GetSessionCookieName( string givenCookieName = null)
        {
            if (givenCookieName == null)
                return SessionManager.defaultCookieName;
            else
                return givenCookieName;
        }

        public static string CreateNewSession()
        {
            string strRet = FnvHash.GetUniqueId(); 

            if (!SessionManager.sessionsBag.TryAdd(strRet, new SessionContainer(strRet)))
                Console.WriteLine("Could not add SessionContainer to sessions bag - sessionId=[" + strRet + "]");

            if( !SessionManager.sessionThreadFlag )
            {
                SessionManager.sessionThread = new Thread(new ThreadStart(ExpireIdleSessions));
                SessionManager.sessionThread.Start();
                SessionManager.sessionThreadFlag = true;
            }

            return strRet;
        }

        public static bool DeleteSession(string sessionId)
        {
            bool bRet = false;

            if (SessionManager.sessionsBag.ContainsKey(sessionId))
            {
                bRet = true;

                if( !SessionManager.sessionsBag.TryRemove(sessionId, out SessionContainer container) )
                    Console.WriteLine("On DeleteSession - Could not remove SessionContainer from sessions bag - sessionId=[" + sessionId + "]");
            }
            return bRet;
        }

        public static object GetValue(string sessionId, string key)
        {
            object objRet = null;

            if (SessionManager.sessionsBag.ContainsKey(sessionId))
            {
                SessionContainer sessionContainer = (SessionContainer)SessionManager.sessionsBag[sessionId];

                if (sessionContainer.NeedExpireSession(SessionManager.sessionMaxIdleTime))
                {
                    if (!SessionManager.sessionsBag.TryRemove(sessionId, out SessionContainer container))
                        Console.WriteLine("On GetValue - Could not remove SessionContainer from sessions bag - sessionId=[" + sessionId + "]");
                }
                else
                    objRet = sessionContainer.GetValue(key);
            }
            return objRet;
        }

        public static string GetValueAsString( string sessionId, string key )
        {
            string strRet = null; ;
            object value = GetValue(sessionId, key);

            if (value != null)
                strRet = value.ToString();

            return strRet;
        }

        public static bool IsSessionExpired( string sessionId )
        {
            bool bRet = false;

            if (SessionManager.sessionsBag.ContainsKey(sessionId))
            {
                SessionContainer sessionContainer = (SessionContainer)SessionManager.sessionsBag[sessionId];

                if (sessionContainer.NeedExpireSession(SessionManager.sessionMaxIdleTime))
                {
                    bRet = true;
                    if (!SessionManager.sessionsBag.TryRemove(sessionId, out SessionContainer container))
                        Console.WriteLine("On IsSessionExpired - Could not remove SessionContainer from sessions bag - sessionId=[" + sessionId + "]");
                }
            }
            else
            {
                bRet = true;
            }

            return bRet;
        }

        public static void SetMaxIdleTimeInMinutes( long duration )
        {
            SessionManager.sessionMaxIdleTime = new TimeSpan(duration);
        }

        public static bool SetValue(string sessionId, string key, object value)
        {
            bool bRet = false;

            if (SessionManager.sessionsBag.ContainsKey(sessionId))
            {
                SessionContainer sessionContainer = (SessionContainer)SessionManager.sessionsBag[sessionId];

                if(!sessionContainer.NeedExpireSession(SessionManager.sessionMaxIdleTime))
                {
                    sessionContainer.SetValue(key, value);
                    bRet = true;
                }
            }

            return bRet;
        }

        // PRIVATE STATIC 
        // The idea is that the thread will sleep based on the shortest timespan for the session in place
        private static void ExpireIdleSessions()
        {
            TimeSpan timeout = SessionManager.sessionMaxIdleTime;
 
            while (sessionThreadFlag)
            { 
                foreach( string key in SessionManager.sessionsBag.Keys )
                {
                    SessionContainer sessionContainer = (SessionContainer)SessionManager.sessionsBag[key];

                    if (sessionContainer.NeedExpireSession(SessionManager.sessionMaxIdleTime))
                    {
                        if (!SessionManager.sessionsBag.TryRemove(key, out SessionContainer container))
                            Console.WriteLine("On ExpireIdleSessions - Could not remove SessionContainer from sessions bag - sessionId=[" + key + "]");

                        if ( SessionManager.sessionsBag.Count == 0 )
                            SessionManager.sessionThreadFlag = false;
                    }
                    else
                    {
                        TimeSpan expirationTime = sessionContainer.GetExpirationDurationInMinutes(SessionManager.sessionMaxIdleTime);

                        if (expirationTime.CompareTo(timeout) < 0)
                            timeout = expirationTime;
                    }
                }
                Thread.Sleep(timeout);
            }
        }

        // PRIVATE CLASSES
        private class ParameterContainer
        {
            private  string parameter = null;

            public string GetParameter()
            {
                string strRet = parameter;

                if (strRet != null)
                    parameter = null;

                return strRet;
            }

            public bool IsParameterPresent()
            {
                return parameter != null;
            }

            public void SetPameter( string value)
            {
                parameter = value;
            }
        }


        private class SessionContainer
        {
            private Hashtable Container = new Hashtable();
            private DateTime LastUpdateTime = DateTime.Now;
            private string SessionId;

            public SessionContainer( string sessionId)
            {
                this.SessionId = sessionId;
            }

            public TimeSpan GetExpirationDurationInMinutes(TimeSpan maxDuration)
            {
                TimeSpan actualDuration = DateTime.Now - this.LastUpdateTime;

                return (maxDuration - actualDuration);
            }

            public object GetValue( string key )
            {
                object objRet = null;

                if (this.Container.ContainsKey(key))
                    objRet = this.Container[key];

                return objRet;
            }

            public string GetValueAsString( string key )
            {
                string strRet = null;
                object value = GetValue(key);

                if (value != null)
                    strRet = value.ToString();

                return strRet;
            }

            public bool IsKeyPresent( string key )
            {
                bool bRet = false;

                bRet = this.Container.ContainsKey(key);

                return bRet;
            }

            public bool NeedExpireSession(TimeSpan maxDuration)
            {
                bool bRet = false;
                TimeSpan actualDuration = DateTime.Now - this.LastUpdateTime;

                if (actualDuration.CompareTo(maxDuration) > 0)
                    bRet = true;

                return bRet;
            }

            public void SetLastUpdateTime()
            {
                this.LastUpdateTime = DateTime.Now;
            }

            public bool SetValue(string key, Object value)
            {
                bool bRet = !IsKeyPresent(key);

                if (this.Container.ContainsKey(key))
                    this.Container[key] = value;
                else
                    this.Container.Add(key, value);

                return bRet;
            }
        }
    }
}
