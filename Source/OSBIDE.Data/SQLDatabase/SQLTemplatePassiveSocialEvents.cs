﻿namespace OSBIDE.Data.SQLDatabase
{
    public static class SQLTemplatePassiveSocialEvents
    {
        public const string Insert = @"if not exists(select 1 from [dbo].DESTINATIONTABLE where EventLogId={0} and UserId={1} and EventDate='{2}') begin insert into [dbo].DESTINATIONTABLE ([EventLogId],[UserId],[EventDate]) values ({0},{1},'{2}') end ";
    }
}
