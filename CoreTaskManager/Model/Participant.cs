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

        public　string GetUserImageUrl(IList<MyIdentityUser> allUsers)
        {
            var selectedUser = allUsers.First(user => user.UserName == UserName);
            return selectedUser.ProfileImageUrl;
        }
    }

    public static class OperateParticipants
    {
        public static IEnumerable<Participant> Take4UsersOfSelectedProgress(this IList<Participant> participants, Progress progress)
        {
            return participants.Where(participant => participant.ProgressId == progress.Id)
                .OrderBy(p => Guid.NewGuid()).Take(4);
        }
    }
}
