

// Generated on 04/17/2013 22:29:53
using System;
using System.Collections.Generic;
using System.Linq;
using BiM.Protocol.Types;
using BiM.Core.IO;
using BiM.Core.Network;

namespace BiM.Protocol.Messages
{
    public class GuildHouseTeleportRequestMessage : NetworkMessage
    {
        public const uint Id = 5712;
        public override uint MessageId
        {
            get { return Id; }
        }
        
        public int houseId;
        
        public GuildHouseTeleportRequestMessage()
        {
        }
        
        public GuildHouseTeleportRequestMessage(int houseId)
        {
            this.houseId = houseId;
        }
        
        public override void Serialize(IDataWriter writer)
        {
            writer.WriteInt(houseId);
        }
        
        public override void Deserialize(IDataReader reader)
        {
            houseId = reader.ReadInt();
            if (houseId < 0)
                throw new Exception("Forbidden value on houseId = " + houseId + ", it doesn't respect the following condition : houseId < 0");
        }
        
    }
    
}