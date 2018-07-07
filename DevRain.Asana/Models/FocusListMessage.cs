using DevRain.WP.Core.MVVM.Messaging;

namespace DevRain.Asana.Models
{
    public class FocusListMessage:BaseMessage
    {
        public FocusListMessage(object sender) : base(sender)
        {
        }

        public bool IsSubtasks { get; set; }
    }

    public class GoToFirstPivotItemMessage:BaseMessage
    {
        public GoToFirstPivotItemMessage(object sender) : base(sender)
        {
        }
    }

    public class HideSubtasksPivotItemMessage:BaseMessage
    {
        public HideSubtasksPivotItemMessage(object sender) : base(sender)
        {
        }
    }

    public class ShowSubtasksPivotItemMessage : BaseMessage
    {
        public ShowSubtasksPivotItemMessage(object sender)
            : base(sender)
        {
        }
    }

    public class TaskStatusCompletedMessage:BaseMessage
    {
        public TaskStatusCompletedMessage(object sender)
            : base(sender)
        {
        }

        public long Id { get; set; }
        public bool IsCompleted { get; set; }
    }
}
