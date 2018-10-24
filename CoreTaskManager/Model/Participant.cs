using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreTaskManager.Model
{
    public class Participant
    {
        public int Id { get; set; }
        public int ProgressId { get; set; }
        public string UserName { get; set; }
        public int CurrentProgress { get; set; }
    }

    public static class OperateParticipants
    {
        public static string GetUserImageUrl(this Participant participant , IList<MyIdentityUser> allUsers)
        {
            var selectedUser = allUsers.First(user => user.UserName == participant.UserName);
            return selectedUser.ProfileImageUrl;
        }

        public static IEnumerable<Participant> Take4UsersOfSelectedProgress(this IList<Participant> participants, Progress progress)
        {
            return participants.Where(participant => participant.ProgressId == progress.Id)
                .OrderBy(p => Guid.NewGuid()).Take(4);
        }
    }
}
