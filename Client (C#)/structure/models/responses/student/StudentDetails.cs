using System.Collections.Generic;

namespace app.structure.models.responses.student
{
    public class StudentTag
    {
        public string _id;
        public Tag tag;
    }

    public class StudentTags
    {
        public Dictionary<string, TagGroup> groups;
        public List<StudentTag> values;
        public long count;
    }

    public class StudentDetails
    {
        public string fullname;
        public List<Guardian> guardians;
        public List<Student> siblings;
        public StudentTags tags;
    }
}
