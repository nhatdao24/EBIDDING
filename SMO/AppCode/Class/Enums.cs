using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMO
{
    public enum WorkFlowType
    {
        Process,
        Activity,
        Level,
        Sender,
        Receiver,
        Com
    }

    public enum ActivityType
    {
        Standard,
        Approve
    }

    public enum ApproveType
    {
        Approve,
        Reject
    }

    public enum Domain
    {
        LANG,
        OBJECT_TYPE,
        ORGANIZE_TYPE,
        CONFIG_TYPE,
        USER_TYPE,
        LOAI_HINH_DOANH_NGHIEP,
        VAI_TRO,
        XEP_HANG
    }
}