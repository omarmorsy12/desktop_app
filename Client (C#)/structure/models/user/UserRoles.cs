using System.Collections.Generic;

namespace app.structure.models.user
{
    public static class UserRoles
    {
        public static string SCHOOL_OWNER = "school_owner";
        public static string HEADMASTER = "headmaster";
        public static string TEACHER_SUPERVISOR = "teacher_supervisor";
        public static string TEACHER = "teacher";
        public static string CO_TEACHER = "co_teacher";
        public static string STAFF = "staff";

        public static List<string> ALL_KEYS = new List<string>() { SCHOOL_OWNER, HEADMASTER, TEACHER_SUPERVISOR, TEACHER, CO_TEACHER, STAFF };
    }
}
