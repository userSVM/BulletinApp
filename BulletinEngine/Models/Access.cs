﻿using BulletinBridge.Data;
using BulletinEngine.Core;
using BulletinEngine.Helpers;
using FessooFramework.Objects.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletinEngine.Entity.Data
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Доступ к Board. </summary>
    ///
    /// <remarks>   SV Milovanov, 01.02.2018. </remarks>
    ///-------------------------------------------------------------------------------------------------

    public class Access : EntityObjectALM<Access, DefaultState>
    {
        #region Properties
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Идентификатор Board. </summary>
        ///
        /// <value> The identifier of the board. </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid BoardId { get; set; }
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Идентификатор пользователя. </summary>
        ///
        /// <value> The identifier of the user. </value>
        ///-------------------------------------------------------------------------------------------------
        public Guid UserId { get; set; }
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Логин для доступа. </summary>
        ///
        /// <value> The login. </value>
        ///-------------------------------------------------------------------------------------------------
        public string Login { get; set; }
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Пароль для доступа. </summary>
        ///
        /// <value> The password. </value>
        ///-------------------------------------------------------------------------------------------------
        public string Password { get; set; }
        /// <summary>
        /// Телефон привязанный к аккаунту
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Заблокирован пользователем - не используем больше ни где
        /// </summary>
        public bool HasBlocked { get; set; }
        public DateTime? LastPublication { get; set; }

        public DateTime? GenerationCheckLast { get; set; }
        public DateTime? GenerationCheckNext { get; set; }
#if DEBUG
        private static int GenerationCheckPeriod = 1;
#else
        private static int GenerationCheckPeriod = 60 * 60 * 24;
#endif

        /// <summary>
        /// Количество просмотров
        /// </summary>
        public int Views { get; set; }
        /// <summary>
        /// Количество звонков
        /// </summary>
        public int Calls { get; set; }
        /// <summary>
        /// Количество сообщений
        /// </summary>
        public int Messages { get; set; }

        /// <summary>
        /// Дата последнего сообщения
        /// </summary>
        public DateTime? LastMessage { get; set; }

        /// <summary>
        /// Используется ли переадресация
        /// </summary>
        public bool IsForwarding { get; set; }

        #endregion
        #region Another methos
        public void SetGenerationCheck()
        {
            GenerationCheckLast = DateTime.Now;
            GenerationCheckNext = GenerationCheckLast.Value.AddSeconds(GenerationCheckPeriod);
            StateEnum = StateEnum;
            BCT.SaveChanges();
        }
        public void SetLastPublication()
        {
            LastPublication = DateTime.Now;
            StateEnum = StateEnum;
            BCT.SaveChanges();
        }
        #endregion

        #region ALM
        protected override IEnumerable<EntityObjectALMConfiguration<Access, DefaultState>> Configurations => Enumerable.Empty<EntityObjectALMConfiguration<Access, DefaultState>>();

        protected override int GetStateValue(DefaultState state)
        {
            return (int)state;
        }

        protected override Access SetValueDefault(Access arg1, Access arg2)
        {
            arg1.Id = arg2.Id;
            arg1.BoardId = arg2.BoardId;
            arg1.UserId = arg2.UserId;
            arg1.Login = arg2.Login;
            arg1.Password = arg2.Password;
            arg1.GenerationCheckLast = arg2.GenerationCheckLast;
            arg1.GenerationCheckNext = arg2.GenerationCheckNext;
            arg1.LastPublication = arg2.LastPublication;
            arg1.LastMessage = arg2.LastMessage;
            arg1.IsForwarding = arg2.IsForwarding;

            return arg1;
        }
        #endregion

        #region Creators
        protected override IEnumerable<EntityObjectALMCreator<Access>> CreatorsService => new[]
        {
            EntityObjectALMCreator<Access>.New<AccessCache>(ToCache, ToEntity, new Version(1,0,0,0)),
        };
        private Access ToEntity(AccessCache cache, Access entity)
        {
            entity.BoardId = cache.BoardId;
            entity.Login = cache.Login;
            entity.Password = cache.Password;
            entity.Phone = cache.Phone;
            entity.HasBlocked = cache.HasBlocked;
            entity.Views = cache.Views;
            entity.Calls = cache.Calls;
            entity.Messages = cache.Messages;
            entity.LastMessage = cache.LastMessage;
            entity.IsForwarding = cache.IsForwarding;
            return entity;
        }
        internal static AccessCache ToCache(Access obj)
        {
            var board = BCT.Context.BulletinDb.Boards.Find(obj.BoardId);
            var result = new AccessCache();
            if (board != null)
            {
                result.BoardName = board.Name;
            }
            result.BoardId = obj.BoardId;
            result.Login = obj.Login;
            result.Password = obj.Password;
            result.Phone = obj.Phone;
            result.HasBlocked = obj.HasBlocked;
            result.StateDesc = obj.StateEnum.ToString();
            result.Views = obj.Views;
            result.Calls = obj.Calls;
            result.Messages = obj.Messages;
            result.LastMessage = obj.LastMessage;
            result.IsForwarding = obj.IsForwarding;
            return result;
        }
        #endregion

        #region DataService -- Methods
        public override IEnumerable<EntityObject> _CollectionObjectLoad()
        {
            var workStates = new[]
            {
                (int)AccessState.Unchecked,
            };
            var result = Enumerable.Empty<EntityObject>();
            BCT.Execute(c =>
            {
                var id = c._SessionInfo.HashUID;
                var id2 = c._SessionInfo.SessionUID;
                if (id == "Engine")
                    result = c.BulletinDb.Accesses
                    .Where(q => workStates.Contains(q.State)).ToArray();
                else
                {
                    result = c.BulletinDb.Accesses.Where(q => q.UserId == c.UserId).ToArray();
                }
            });
            return result;
        }
        public override IEnumerable<TDataModel> _CacheSave<TDataModel>(IEnumerable<TDataModel> objs)
        {
            var result = Enumerable.Empty<TDataModel>();
            BCT.Execute(d =>
            {
                var access = objs.FirstOrDefault() as Access;
                var dbAccess = d.BulletinDb.Accesses.FirstOrDefault(q => q.Login == access.Login && q.Password == access.Password);
                if (dbAccess == null)
                {
                    access.BoardId = d.BulletinDb.Boards.FirstOrDefault().Id;
                    access.UserId = d.UserId;
                    access.StateEnum = DefaultState.Created;
                    d.SaveChanges();
                }
                else
                {
                    dbAccess.Login = access.Login;
                    dbAccess.Password = access.Password;
                    dbAccess.Views = access.Views;
                    dbAccess.Calls = access.Calls;
                    dbAccess.Messages = access.Messages;
                    dbAccess.LastMessage = access.LastMessage;
                    dbAccess.IsForwarding = access.IsForwarding;
                    d.SaveChanges();
                }
                result = new TDataModel[] { access as TDataModel };
            });
            return result;
        }
        #endregion

        #region Custom query
        public override IEnumerable<EntityObject> CustomCollectionLoad(string code, string sessionUID = "", string hashUID = "", IEnumerable<EntityObject> obj = null, IEnumerable<Guid> id = null)
        {
            var result = Enumerable.Empty<EntityObject>();
            BCT.Execute(c =>
            {
                //Пока не заморачивался - передаётся базовый объект и требуется привести к типу
                var entities = Enumerable.Empty<Access>();
                if (obj.Any())
                    entities = obj.Select(q => (Access)q).ToArray();
                switch (code)
                {
                    case "All":
                        result = AccessHelper.All();
                        break;
                    case "AddAvito":
                        result = new[] { AccessHelper.AddAvito(entities.FirstOrDefault()) };
                        break;
                    case "ActivateAccess":
                        AccessHelper.ActivateAccess(entities.FirstOrDefault());
                        break;
                    case "GetAccessStatistics":
                        AccessHelper.GetAccessStatistics(entities.FirstOrDefault());
                        break;
                    case "Remove":
                        AccessHelper.Remove(entities);
                        break;
                    case "Enable":
                        AccessHelper.Enable(id);
                        break;
                    case "Disable":
                        AccessHelper.Disable(id);
                        break;
                    default:
                        break;
                }
            });
            return result;
        }
        #endregion
    }

}