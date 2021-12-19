using app.structure.models.general;

namespace app.structure.models.responses.startup
{
    public class SchoolInformationDataAddress
    {
        public string street_name;
        public string building_number;
        public string city;
    }
    public class SchoolInformationData
    {
        public string ref_id;
        public ItemTranslation name;
        public SchoolInformationDataAddress address;
        public string phone_number;
        public string logo;
    }
}
