using OFOS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Core
{
    public interface INotificationService
    {
        void ConsumeEmailMessage(string emailMessage);
    }
}
