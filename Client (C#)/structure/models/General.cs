namespace app.structure.models.general
{
    public class Name
    {
        public string first;
        public string last;
    }

    public class ItemTranslation
    {
        public string EN;
        public string AR;

        public ItemTranslation(string EN, string AR)
        {
            this.EN = EN;
            this.AR = AR;
        }

        public ItemTranslation()
        {
        }
    }
    
    public class Address
    {
        public string street_name;
        public string building_number;
        public string city;
    }

}
