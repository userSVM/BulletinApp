﻿using System;
using System.Collections.Generic;
using BulletinBridge.Data;

namespace BulletinWebWorker.Containers.Base
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Управляет созданием и изменение буллетинов на борде </summary>
    ///
    /// <remarks>   SV Milovanov, 14.02.2018. </remarks>
    ///-------------------------------------------------------------------------------------------------

    abstract class BulletinPackageContainerBase
    {
        public abstract Guid Uid { get; }
        public abstract void AddBulletins(IEnumerable<BulletinBridge.Data.TaskCache_old> tasks);
        public abstract void CheckModerationState(IEnumerable<TaskCache_old> tasks);
        public abstract void GetBulletinList(IEnumerable<TaskCache_old> tasks);
        public abstract void GetBulletinDetails(IEnumerable<TaskCache_old> tasks);
        public abstract void EditBulletins(IEnumerable<TaskCache_old> tasks);
    }
}