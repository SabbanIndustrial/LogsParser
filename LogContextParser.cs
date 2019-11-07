using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogsParser
{
    class LogContextParser
    {
        public List<LogDocument> Documents
        {
            get;
            private set;
        }
        public LogContextParser(LogDocument document)
        {
            Documents = new List<LogDocument>();
            Documents.Add(document);
        }
        public LogContextParser(List<LogDocument> documents)
        {
            Documents = new List<LogDocument>();
            Documents.AddRange(documents);
        }
        public LogContextParser()
        {
            Documents = new List<LogDocument>();
        }
        public void AddDocument(LogDocument document)
        {
            Documents.Add(document);
            document.ParseDocument();
        }




        public List<LogBlock> GetAllBlocksWithGuid(string guid)
        {
            List<LogBlock> toReturn = new List<LogBlock>();
            foreach (var item in Documents)
            {
                toReturn.AddRange(item.Blocks.Where(b => b.Guid == guid).ToList());
            }
            return toReturn;
        }
        public List<LogBlock> GetAllBlocksWithOperId(string operId)
        {
            List<LogBlock> toReturn = new List<LogBlock>();
            foreach (var item in Documents)
            {
                toReturn.AddRange(item.Blocks.Where(b => b.OperId == operId).ToList());
            }
            return toReturn;
        }
        public List<LogBlock> GetAllBlocksWithOrderId(string orderId)
        {
            List<LogBlock> toReturn = new List<LogBlock>();
            foreach (var item in Documents)
            {
                toReturn.AddRange(item.Blocks.Where(b => b.OrderId == orderId).ToList());
            }
            return toReturn;
        }
        public List<LogBlock> GetAllBlocksWithRRN(string RRN)
        {
            List<LogBlock> toReturn = new List<LogBlock>();
            foreach (var item in Documents)
            {
                toReturn.AddRange(item.Blocks.Where(b => b.RRN == RRN).ToList());
            }
            return toReturn;
        }
        public List<LogBlock> GetAllBlocksWithIDPlatKlienta(string Id_Plat_Klienta)
        {
            List<LogBlock> toReturn = new List<LogBlock>();
            foreach (var item in Documents)
            {
                toReturn.AddRange(item.Blocks.Where(b => b.ID_Plat_Klienta == Id_Plat_Klienta).ToList());
            }
            return toReturn;
        }
        public List<LogBlock> GetAllBlocksWithIDSiteUser(string IdSiteUser)
        {
            List<LogBlock> toReturn = new List<LogBlock>();
            foreach (var item in Documents)
            {
                toReturn.AddRange(item.Blocks.Where(b => b.IdSiteUser == IdSiteUser).ToList());
            }
            return toReturn;
        }

        public List<LogBlock> CreateBlockChain(string RRN)
        {
            List<string> guids = new List<string>();
            List<string> operIds = new List<string>();
            List<string> orderIds = new List<string>();
            List<string> IdPlatKlients = new List<string>();
            List<string> IdSiteUsers = new List<string>();

            List<LogBlock> toReturn = new List<LogBlock>();
            toReturn.AddRange(GetAllBlocksWithRRN(RRN));
            foreach (var item in toReturn)
            {
                guids.Add(item.Guid);
            }

            foreach (var item in guids.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithGuid(item));
            }
            foreach (var item in toReturn)
            {
                if (item.OperId != "")
                {
                    operIds.Add(item.OperId);

                }
                if (item.OrderId != "")
                {
                    orderIds.Add(item.OrderId);
                }
                if (item.ID_Plat_Klienta != "")
                {
                    IdPlatKlients.Add(item.ID_Plat_Klienta);
                }
                if (item.IdSiteUser != "")
                {
                    IdSiteUsers.Add(item.IdSiteUser);
                }
            }
            foreach (var item in IdPlatKlients.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithIDPlatKlienta(item));
            }
            foreach (var item in IdSiteUsers.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithIDSiteUser(item));
            }
            foreach (var item in operIds.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithOperId(item));

            }
            foreach (var item in orderIds.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithOrderId(item));

            }
            toReturn = toReturn.Distinct().ToList();
            guids.Clear();
            foreach (var item in toReturn)
            {
                guids.Add(item.Guid);
            }
            foreach (var item in guids.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithGuid(item));
            }
            operIds.Clear();
            orderIds.Clear();
            foreach (var item in toReturn)
            {
                if (item.ID_Plat_Klienta != "")
                {
                    IdPlatKlients.Add(item.ID_Plat_Klienta);
                }
                if (item.OperId != "")
                {
                    operIds.Add(item.OperId);

                }
                if (item.OrderId != "")
                {
                    orderIds.Add(item.OrderId);
                }
                if (item.IdSiteUser != "")
                {
                    IdSiteUsers.Add(item.IdSiteUser);
                }
            }
            foreach (var item in IdPlatKlients.Distinct().ToList())
            {
                toReturn.AddRange(GetAllBlocksWithIDPlatKlienta (item));

            }
            foreach (var item in operIds.Distinct().ToList())
            {
                toReturn.AddRange(GetAllBlocksWithOperId(item));

            }
            foreach (var item in orderIds.Distinct().ToList())
            {
                toReturn.AddRange(GetAllBlocksWithOrderId(item));

            }
            foreach (var item in IdSiteUsers.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithIDSiteUser(item));
            }
            guids.Clear();
            foreach (var item in toReturn)
            {
                guids.Add(item.Guid);
            }
            foreach (var item in guids.Distinct())
            {
                toReturn.AddRange(GetAllBlocksWithGuid(item));
            }

            toReturn.Sort();
            return toReturn.Distinct(). ToList();
        }


    }



    public struct BlockChainContext
    {
        public string RRN { get; set; }





    }
}
