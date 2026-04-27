
delete from ChannelData
Declare @TagID int;
Set @TagID = 31;
Declare @theTime DATETIME
Set @theTime = '2026-01-01 00:00';
Declare @endTime DATETIME
Set @endTime = '2026-12-31 23:00';

Declare @v float;
Set @v = 0.0;

--DECLARE @y INT=2026, @m INT=4, @d INT=1, @h INT=0, @min INT=0

While @theTime <= @endTime
Begin 
    set @v = rand() * 30 + 10
    INSERT INTO [dbo].[ChannelData]
           ([tagID]
           ,[Time]
           ,[Value]
           ,[Status])
        VALUES
           (@TagID,
           @theTime,
           @v,
           0) 
   Set @theTime = DATEADD(hour, 1, @theTime )
 End


Set @TagID = 29;
Set @theTime = '2026-04-01 00:00';
Set @endTime = '2026-04-30 23:00';
While @theTime <= @endTime
Begin 
    set @v = rand() * 30 + 10
    INSERT INTO [dbo].[ChannelData]
           ([tagID]
           ,[Time]
           ,[Value]
           ,[Status])
        VALUES
           (@TagID,
           @theTime,
           @v,
           0) 
   Set @theTime = DATEADD(minute, 1, @theTime )
 End
 
 SELECT [tagID]
      ,[Time]
      ,[Value]
      ,[Status]
  FROM [Scada].[dbo].[ChannelData]
  where tagID = 29
  ORDER BY time desc
