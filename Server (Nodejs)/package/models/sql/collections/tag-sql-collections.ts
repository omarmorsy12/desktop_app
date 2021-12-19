import { OwnedTagsSQLDocument } from "../documents/tag/owned-tags-sql-doc";
import { TagSQLDocument } from "../documents/tag/tag-sql-doc";

export class TagSQLCollections {
    owned = <OwnedTagsSQLDocument>{}
    values = <TagSQLDocument>{}
}