// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Insight.DataService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Iot.Insight.DataService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.AspNetCore.Hosting;

    using global::Iot.Common;

    [Route("api/[controller]")]
    public class EntitiesController : Controller
    {
        private readonly IApplicationLifetime appLifetime;
        private readonly IReliableStateManager stateManager;
        private readonly StatefulServiceContext context;

        public EntitiesController(IReliableStateManager stateManager, StatefulServiceContext context, IApplicationLifetime appLifetime)
        {
            this.stateManager = stateManager;
            this.appLifetime = appLifetime;
            this.context = context;
        }


        [HttpGet]
        [Route("{name}/byId/{id}")]
        public async Task<IActionResult> ReadEntityById( string name, string id)
        {
            User userRet = null;
            IReliableDictionary<string, User> entitiesDictionary =  await this.stateManager.GetOrAddAsync<IReliableDictionary<string, User>>(Names.EntitiesDictionaryName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                try
                {
                    var result = await entitiesDictionary.TryGetValueAsync(tx, id);

                    if (result.HasValue)
                    {
                        userRet = new User();

                        userRet.Id = id;
                        userRet.FirstName = result.Value.FirstName;
                        userRet.LastName = result.Value.LastName;
                        userRet.Password = result.Value.Password;
                        userRet.PasswordCreated = result.Value.PasswordCreated;
                        userRet.Username = result.Value.Username;
                    }
                    await tx.CommitAsync();
                }
                catch (TimeoutException te)
                {
                    // transient error. Could Retry if one desires .
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - ReadEntityById - TimeoutException : Message=[{te.ToString()}]");
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - ReadEntityById - General Exception - Message=[{0}]", ex);
                    tx.Abort();
                }
            }

            return this.Ok(userRet);
        }

        [HttpGet]
        [Route("{name}/byIdentity/{key}")]
        public async Task<IActionResult> ReadEntityByIdentity(string name, string key)
        {
            UserProfile userProfile = new UserProfile();
            IReliableDictionary<string, string> identitiesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Names.IdentitiesDictionaryName);
            IReliableDictionary<string, User> entitiesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, User>>(Names.EntitiesDictionaryName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                try
                {
                    var result = await identitiesDictionary.TryGetValueAsync(tx, key);

                    if (result.HasValue)
                    {
                        var userResult = await entitiesDictionary.TryGetValueAsync(tx, result.Value.ToString());

                        userProfile.FirstName = userResult.Value.FirstName;
                        userProfile.LastName = userResult.Value.LastName;
                        userProfile.UserName = userResult.Value.Username;
                        userProfile.Password = userResult.Value.Password;
                    }
                    await tx.CommitAsync();
                }
                catch (TimeoutException te)
                {
                    // transient error. Could Retry if one desires .
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - ReadEntityByIdentity - TimeoutException : Message=[{te.ToString()}]");
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - ReadEntityByIdentity - General Exception - Message=[{0}]", ex);
                    tx.Abort();
                }
            }

            return this.Ok(userProfile);
        }


        [HttpPost]
        [Route("{name}/withIdentity/{key}")]
        public async Task<IActionResult> CreateEntity(string name, string key, [FromBody]UserProfile userProfile )
        {
            bool bRet = false;
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(key) )
            {
                return this.BadRequest();
            }

            if( userProfile != null )
                Debug.WriteLine("On CreateEntity postContent=[" + userProfile.ToString() + "]");
            else
                Debug.WriteLine("On CreateEntity postContent=[ userProfile is null ]");

            if (userProfile == null)
            {
                return this.BadRequest();
            }

            string id = FnvHash.GetUniqueId();
            User user = new User();

            user.Id = id;
            user.Username = userProfile.UserName;
            user.FirstName = userProfile.FirstName;
            user.LastName = userProfile.LastName;
            user.Password = userProfile.Password;
            user.PasswordCreated = true;

            IReliableDictionary<string, string> identitiesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Names.IdentitiesDictionaryName);
            IReliableDictionary<string, User> entitiesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, User>>(Names.EntitiesDictionaryName);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                int retryCount = 1;

                while( retryCount > 0)
                {
                    try
                    {
                        await identitiesDictionary.AddAsync(tx, userProfile.UserName, id);
                        await entitiesDictionary.AddAsync(tx, id, user);
                        // Commit
                        await tx.CommitAsync();
                        retryCount = 0;
                    }
                    catch (TimeoutException te)
                    {
                        // transient error. Could Retry if one desires .
                        ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - CreateEntity(Save) - TimeoutException : Retry Count#{retryCount}: Message=[{te.ToString()}]");

                        if(global::Iot.Common.Names.TransactionsRetryCount > retryCount )
                        {
                            retryCount = 0;
                        }
                        else
                        {
                            retryCount++;

                            await Task.Delay(global::Iot.Common.Names.TransactionRetryWaitIntervalInMills * (int)Math.Pow(2,retryCount));
                        }
                    }
                    catch (Exception ex)
                    {
                        ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - CreateEntity(Save) - General Exception - Message=[{0}]", ex);
                        retryCount = 0;
                        tx.Abort();
                    }
                }
            }

            // now let's check if the commits have finished
            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                try
                {
                    bool keepReading = true;
                    while (keepReading)
                    {
                        var result = await identitiesDictionary.TryGetValueAsync(tx, userProfile.UserName);

                        if (result.Value.Equals(id))
                        {
                            await tx.CommitAsync();
                            bRet = true;
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (TimeoutException te)
                {
                    // transient error. Could Retry if one desires .
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - CreateEntity(Wait Save) - TimeoutException : Message=[{te.ToString()}]");
                }
                catch (Exception ex)
                {
                    ServiceEventSource.Current.ServiceMessage(this.context, $"DataService - CreateEntity(Wait Save) - General Exception - Message=[{0}]", ex);
                    tx.Abort();
                }
            }

            return this.Ok(bRet);
        }
    }
}
