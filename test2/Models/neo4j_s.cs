using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Neo4j.Driver.V1;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using Gma.DataStructures.StringSearch;
using System.ComponentModel.DataAnnotations;

namespace test2.Models
{



    public class neo4j_s
    {
        static neo4j_s()
        {
        }
        public static IDriver db_Driver = GraphDatabase.Driver("bolt://localhost", AuthTokens.Basic("neo4j", "123456"));
        public static neo4j_s db = null;
        //static neo4j_s GetInstance()
        //{
        //    if (db == null)
        //    {
        //        db = new neo4j_s();
        //        db_Driver = GraphDatabase.Driver("bolt://localhost", AuthTokens.Basic("neo4j", "qwe3809696"));
        //    }
        //    return db;
        //}
        public static IStatementResult query(string query)
        {
            IStatementResult result;
            using (var session = db_Driver.Session())
            {
                result = session.Run(query);
            }
            return result;
        }
        public static void Del_node(string id)
        {
            query("match (a) where id(a)=" + id + " detach delete a");
        }
    }



    public class user : neo4j_s
    {
        static user() { }
        public static List<INode> users()
        {
            return query("match (a:User)  return a").Select(a => (INode)a["a"]).ToList();
        }
        public static INode users_find(string id)
        {
            List<INode> result = query("match(a:User{Id:'" + id + "'})  return a").Select(a => (INode)a["a"]).ToList();
            if (result.Count() == 0)
                return null;
            else
                return result.First();
        }
        public static void Del_node(string id_user, string id)
        {
            query("match (n:User{Id:'" + id_user + "'})-[:product]->(a:ad) where id(a)=" + id + " detach delete a");
        }
        public static string Ad_user_mail(long id_ad)
        {
            try
            {
                return query("Match (a:User)-->(n:ad) where id(n)=" + id_ad.ToString() + " return a.Email").Select(a => a["a.Email"]).First().As<string>();
            }
            catch
            {
                return null;
            }
        }
    }
    public class Ad : neo4j_s
    {
        //Gma.DataStructures.StringSearch.SuffixTrie<KeyValuePair<string, long>> Search;
        private static Ad ad = null;
        //private static int Suffix_lenght = 0;
        private Ad()
        {

            //Search = new Gma.DataStructures.StringSearch.SuffixTrie<KeyValuePair<string, long>>(Suffix_lenght);
            //var res = query("match (a:ad)  return a.header,id(a)").Select(a => new KeyValuePair<string, long>(a["a.header"].As<string>(), a["id(a)"].As<long>())).ToList();
            //foreach (var i in res)
            //    Search.Add(i.Key, new KeyValuePair<string, long>(i.Key, i.Value));

        }
        public bool create(string id_user, string name_types_standart, List<string> name_tags, string header, string data, float Price)
        {
            bool test = true;
            using (var session = db_Driver.Session().BeginTransaction())
            {
                var id = session.Run("match(n:User{Id:'" + id_user + "'})  with n Create(a:ad{time:timestamp(),header:'" + header + "',data:'" + data + "', Price:" + Price + "}) merge (n)-[:product]->(a) return a");
                var id_elem = id.First()["a"].As<INode>().Id;
                var count_el = session.Run("match(a:ad),(n:type_standart{name:'" + name_types_standart + "'}) where id(a)=" + id_elem + "  merge (a)-[:type]->(n) return n");
                if (count_el.Count() == 0)
                {
                    test = false;
                    session.Failure();
                }
                else
                {
                    foreach (string name_t in name_tags)
                    {
                        session.Run("match(a:ad) where id(a)=" + id_elem + " with a merge (n:tag{name:'" + name_t + "'}) merge (a)-[:tag]->(n)");
                    }
                    session.Success();
                }

            }
            return test;
        }
        public List<Ad_model> Search_all(string Search, int lim = 25, int skip = 0)
        {
            var test_q = Ad_model_list(query("MATCH(n: ad) where n.header=~'.*(?i)" + Search + ".*'  RETURN n ORDER BY n.time desc skip " + skip + " LIMIT " + lim + " ").Select(n => (INode)n["n"]).ToList());

            return test_q;
        }
        public List<Ad_model> ads()
        {
            return Ad_model_list(query("match (a:ad)  return a ORDER BY a.time desc").Select(a => (INode)a["a"]).ToList());
        }
        public List<Ad_model> ads(int limit, int skip)
        {
            return Ad_model_list(query("match (a:ad)  return a ORDER BY a.time desc skip " + skip + " LIMIT " + limit + " ").Select(a => (INode)a["a"]).ToList());
        }
        public List<Ad_model> ads_tag(string tag, int lim = 25, int skip = 0)
        {
            return Ad_model_list(query("match (n:ad)-[:tag]-(b:tag{name:'" + tag + "'}) RETURN n ORDER BY n.time desc skip " + skip + " LIMIT " + lim + " ").Select(a => (INode)a["n"]).ToList());
        }
        public List<Ad_model> ads_type(string type, int lim = 25, int skip = 0)
        {
            return Ad_model_list(query("match (n:ad)-[:type]-(b:type_standart{name:'" + type + "'})  RETURN n ORDER BY n.time desc skip " + skip + " LIMIT " + lim + " ").Select(a => (INode)a["n"]).ToList());
        }
        public List<Ad_model> ads_type_tage(string type, string tag)
        {
            return Ad_model_list(query("MATCH(n)-[r: type]-(a:type_standart{name:'" + type + "'}) where(n)-[:tag]-(:tag{name:'" +
                tag + "'})  RETURN n").Select(a => (INode)a["n"]).ToList());
        }
        public static Ad GetInstance()
        {
            if (ad == null)
            {
                lock (typeof(Ad))
                {
                    if (ad == null)
                        ad = new Ad();
                }
            }
            return Ad.ad;

        }
        private List<Ad_model> ads_name(string header)
        {
            return Ad_model_list(query("match (a:ad{header:'" + header + "'})  return a  ").Select(a => (INode)a["a"]).ToList());
        }



        private static List<Ad_model> Ad_model_list(List<INode> data)
        {
            List<Ad_model> models = new List<Ad_model>();
            foreach (var i in data)
            {
                Ad_model model = new Ad_model();
                model.id = i.Id;
                model.header = i.Properties["header"].As<string>();
                model.data = i.Properties["data"].As<string>();
                model.Price = i.Properties["Price"].As<float>();
                model.Email = user.Ad_user_mail(i.Id);
                foreach (var j in type_standart.types(i.Id))
                {
                    model.type = j.Properties["name"].As<string>();
                }
                model.tags_list = new List<string>();
                foreach (var j in tag.tags(i.Id))
                {
                    model.tags_list.Add(j.Properties["name"].As<string>());
                }
                models.Add(model);
            }
            return models;
        }


        public List<Ad_model> User_ad(string id_user)
        {
            return Ad_model_list(query("match(a:User{Id:'" + id_user + "'})-[:product]-(n:ad)  return n").Select(a => (INode)a["n"]).ToList());
        }
        public Ad_model ad_id(string id)
        {

            var data = Ad_model_list(query("match (a:ad) where id(a)=" + id + "  return a ").Select(a => (INode)a["a"]).ToList());
            if (data.Count != 0)
                return data.First();
            else return new Ad_model();
        }
        public void Ad_edit(string id_user, Ad_model edit)
        {
            List<string> Teg = edit.tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (this.Ad_user_id(edit.id) == id_user)
                if (this.create(id_user, edit.type, Teg, edit.header, edit.data, edit.Price))
                {
                    neo4j_s.Del_node(edit.id.ToString());
                }
        }
        public string Ad_user_id(long id_ad)
        {
            try
            {
                return query("Match (a:User)-->(n:ad) where id(n)=" + id_ad.ToString() + " return a.Id").Select(a => a["a.Id"]).First().As<string>();
            }
            catch
            {
                return null;
            }
        }
    }

    public class Ad_model
    {

        [Required]
        [Display(Name = "data")]
        [DataType(DataType.MultilineText)]
        public string data { get; set; }
        [Required]
        [Display(Name = "header")]
        public string header { get; set; }
        [Required]
        [Display(Name = "type")]
        public string type { get; set; }
        [Required]
        [Display(Name = "tags")]
        public string tags { get; set; }


        [Required]
        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public float Price { get; set; }

        public long id { get; set; }

        public List<string> tags_list { get; set; }
        public string Email { get; set; }
    }
    public class tag : neo4j_s
    {
        static Func<IRecord, Tag_model> func_Select;
        static tag()
        {
            func_Select = (a) => new Tag_model { name = ((INode)a["n"])["name"].As<string>(), id = ((INode)a["n"]).Id };
        }
        public static List<Tag_model> Search_all(string Search, int lim = 25)
        {

            var test_q = (query("MATCH(n:tag) where n.name =~'.*(?i)" + Search + ".*'  RETURN n LIMIT " + lim + " ").Select(n => new Tag_model { name = ((INode)n["n"])["name"].As<string>(), id = ((INode)n["n"]).Id }).ToList());

            return test_q;
        }
        public static List<Tag_model> tag_id(long id)
        {
            return query("match (n:tag) where id(n)=" + id + " return n").Select(func_Select).ToList();
        }
        public static List<INode> tags(long id)
        {
            return query("match (a:ad)-[:tag]-(n:tag) where id(a)=" + id + " return n").Select(a => (INode)a["n"]).ToList();
        }
        public static void tag_Add(string tag)
        {
            query("merge (a:tag{name:'" + tag + "'})");
        }
        public static void tag_del(string tag)
        {
            query("match (a:tag{name:'" + tag + "'}) detach delete a");
        }
        public static List<Tag_model> type_name_rel(string type)
        {
            return query("match (a:type_standart{name:'" + type + "'})-[:rel]-(n:tag) return n").Select(func_Select).ToList();

        }

    }
    public class Tag_model
    {
        [Required]
        [Display(Name = "name")]
        public string name;
        public long id;
    }
    public class Type_model
    {
        [Required]
        [Display(Name = "name")]
        public string name;
        public long id;
        [Required]
        [Display(Name = "tags")]
        public List<string> tags;

    }
    public class type_standart : neo4j_s
    {

        static List<Type_model> name;
        public static List<Type_model> get_name_list()
        {
            return name;
        }
        static type_standart()
        {
            name = Type_node_to_list();
        }
        public static void update()
        {
            name = Type_node_to_list();

        }
        public static List<INode> type_standarts()
        {
            return query("match (n:type_standart) return n").Select(a => (INode)a["n"]).ToList();

        }
        public static List<INode> types(long id)
        {

            return query("match (a:ad)-[:type]->(n:type_standart) where id(a)=" + id + " return n").Select(a => (INode)a["n"]).ToList();
        }

        public static List<Type_model> Type_node_to_list()
        {
            var inf = type_standarts().Select(a => new Type_model { name = a.Properties["name"].As<string>(), id = a.Id }).ToList();
            foreach (var i in inf)
            {
                i.tags = tag.type_name_rel(i.name).Select(a => a.name).ToList();
            }
            return inf;
        }


        public static void type_tag_rel(string type, string tag)
        {
            query("match (a:type_standart{name:'" + type + "'}),(n:tag{name:'" + tag + "'}) merge (a)-[:rel]->(n)");
        }
        public static void type_tag_rel_del(string type, string tag)
        {
            query("match (a:type_standart{name:'" + type + "'})-[k:rel]->(n:tag{name:'" + tag + "'}) delete k");
        }

        public static void type_Add(string type)
        {
            query("merge (a:type_standart{name:'" + type + "'})");
        }
        public static void type_del(string type)
        {
            query("match (a:type_standart{name:'" + type + "'}) detach delete a");
        }

    }






    //public class neo4j_s
    //{
    //    static neo4j_s()
    //    {
    //    }
    //    public static IDriver db_Driver = GraphDatabase.Driver("bolt://localhost", AuthTokens.Basic("neo4j", "123456"));
    //    public static neo4j_s db = null;
    //    static neo4j_s GetInstance()
    //    {
    //        if (db == null)
    //        {
    //            db = new neo4j_s();
    //            db_Driver = GraphDatabase.Driver("bolt://localhost", AuthTokens.Basic("neo4j", "qwe3809696"));
    //        }
    //        return db;
    //    }
    //    public static IStatementResult query(string query)
    //    {
    //        IStatementResult result;
    //        using (var session = db_Driver.Session())
    //        {
    //            result = session.Run(query);
    //        }
    //        return result;
    //    }
    //    public static void Del_node(string id)
    //    {
    //        query("match (a) where id(a)=" + id + " detach delete a");
    //    }
    //}
    //public class user : neo4j_s
    //{
    //    static user() { }
    //    public static List<INode> users()
    //    {
    //        return query("match (a:User)  return a").Select(a => (INode)a["a"]).ToList();
    //    }
    //    public static INode users_find(string id)
    //    {
    //        List<INode> result = query("match(a:User{Id:'" + id + "'})  return a").Select(a => (INode)a["a"]).ToList();
    //        if (result.Count() == 0)
    //            return null;
    //        else
    //            return result.First();
    //    }

    //    public static string Ad_user_mail(long id_ad)
    //    {
    //        try
    //        {
    //            return query("Match (a:User)-->(n:ad) where id(n)=" + id_ad.ToString() + " return a.Email").Select(a => a["a.Email"]).First().As<string>();
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }

    //    public static void Del_node(string id_user, string id)
    //    {
    //        query("match (n:User{Id:'" + id_user + "'})-[:product]->(a:ad) where id(a)=" + id + " detach delete a");
    //    }
    //}
    //public class Ad : neo4j_s
    //{
    //    Gma.DataStructures.StringSearch.SuffixTrie<KeyValuePair<string, long>> Search;
    //    private static Ad ad = null;
    //    private static int Suffix_lenght = 0;
    //    private Ad()
    //    {

    //        Search = new Gma.DataStructures.StringSearch.SuffixTrie<KeyValuePair<string, long>>(Suffix_lenght);
    //        var res = query("match (a:ad)  return a.header,id(a)").Select(a => new KeyValuePair<string, long>(a["a.header"].As<string>(), a["id(a)"].As<long>())).ToList();
    //        foreach (var i in res)
    //            Search.Add(i.Key, new KeyValuePair<string, long>(i.Key, i.Value));

    //    }
    //public bool create(string id_user, string name_types_standart, List<string> name_tags, string header, string data, float Price)
    //{
    //    bool test = true;
    //    using (var session = db_Driver.Session().BeginTransaction())
    //    {
    //        var id = session.Run("match(n:User{Id:'" + id_user + "'})  with n Create(a:ad{time:timestamp(),header:'" + header + "',data:'" + data + "', Price:" + Price + "}) merge (n)-[:product]->(a) return a");
    //        var id_elem = id.First()["a"].As<INode>().Id;
    //        var count_el = session.Run("match(a:ad),(n:type_standart{name:'" + name_types_standart + "'}) where id(a)=" + id_elem + "  merge (a)-[:type]->(n) return n");
    //        if (count_el.Count() == 0)
    //        {
    //            test = false;
    //            session.Failure();
    //        }
    //        else
    //        {
    //            foreach (string name_t in name_tags)
    //            {
    //                session.Run("match(a:ad) where id(a)=" + id_elem + " with a merge (n:tag{name:'" + name_t + "'}) merge (a)-[:tag]->(n)");
    //            }
    //            session.Success();
    //        }

    //    }
    //    return test;
    //}
    //    public List<Ad_model> Search_all(string Search, int lim = 25)
    //    {
    //        List<Ad_model> data = new List<Ad_model>();
    //        var test_q = Ad_model_list(query("MATCH(n: ad) where n.header=~'.*(?i)" + Search + ".*'  RETURN n LIMIT " + lim + " ").Select(n => (INode)n["n"]).ToList());
    //        var a = this.Search.Retrieve(Search).Select(b => b.Key).ToList();
    //        foreach (var i in a)
    //        {
    //            var inf = this.ads_name(i);
    //            data.AddRange(inf);
    //        }
    //        return test_q;
    //    }
    //    public List<Ad_model> ads()
    //    {
    //        return Ad_model_list(query("match (a:ad)  return a ORDER BY a.time desc").Select(a => (INode)a["a"]).ToList());
    //    }
    //    public List<Ad_model> ads(int limit, int skip)
    //    {
    //        return Ad_model_list(query("match (a:ad)  return a ORDER BY a.time desc skip " + skip + " LIMIT " + limit + " ").Select(a => (INode)a["a"]).ToList());
    //    }
    //    public List<Ad_model> ads_tag(string tag)
    //    {
    //        return Ad_model_list(query("match (a:ad)-[:tag]-(b:tag{name:'" + tag + "'})  return a ORDER BY a.time").Select(a => (INode)a["a"]).ToList());
    //    }
    //    public List<Ad_model> ads_type(string type)
    //    {
    //        return Ad_model_list(query("match (a:ad)-[:type]-(b:type_standart{name:'" + type + "'})  return a").Select(a => (INode)a["a"]).ToList());
    //    }
    //    public List<Ad_model> ads_type_tage(string type, string tag)
    //    {
    //        return Ad_model_list(query("MATCH(n)-[r: type]-(a:type_standart{name:'" + type + "'}) where(n)-[:tag]-(:tag{name:'" +
    //            tag + "'})  RETURN n").Select(a => (INode)a["n"]).ToList());
    //    }
    //    public static Ad GetInstance()
    //    {
    //        if (ad == null)
    //        {
    //            lock (typeof(Ad))
    //            {
    //                if (ad == null)
    //                    ad = new Ad();
    //            }
    //        }
    //        return Ad.ad;

    //    }
    //    private List<Ad_model> ads_name(string header)
    //    {
    //        return Ad_model_list(query("match (a:ad{header:'" + header + "'})  return a  ").Select(a => (INode)a["a"]).ToList());
    //}
    //private static List<Ad_model> Ad_model_list(List<INode> data)
    //{
    //    List<Ad_model> models = new List<Ad_model>();
    //    foreach (var i in data)
    //    {
    //        Ad_model model = new Ad_model();
    //        model.id = i.Id;
    //        model.header = i.Properties["header"].As<string>();
    //        model.data = i.Properties["data"].As<string>();
    //        model.Price = i.Properties["Price"].As<float>();
    //        model.Email = user.Ad_user_mail(i.Id);
    //        foreach (var j in type_standart.types(i.Id))
    //        {
    //            model.type = j.Properties["name"].As<string>();
    //        }
    //        model.tags_list = new List<string>();
    //        foreach (var j in tag.tags(i.Id))
    //        {
    //            model.tags_list.Add(j.Properties["name"].As<string>());
    //        }
    //        models.Add(model);
    //    }
    //    return models;
    //}
    //    public List<Ad_model> User_ad(string id_user)
    //    {
    //        return Ad_model_list(query("match(a:User{Id:'" + id_user + "'})-[:product]-(n:ad)  return n").Select(a => (INode)a["n"]).ToList());
    //    }
    //    public Ad_model ad_id(string id)
    //    {

    //        var data = Ad_model_list(query("match (a:ad) where id(a)=" + id + "  return a ").Select(a => (INode)a["a"]).ToList());
    //        if (data.Count != 0)
    //            return data.First();
    //        else return new Ad_model();
    //    }
    //    public void Ad_edit(string id_user, Ad_model edit)
    //    {
    //        List<string> Teg = edit.tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    //        if (this.Ad_user_id(edit.id) == id_user)
    //            if (this.create(id_user, edit.type, Teg, edit.header, edit.data, edit.Price))
    //            {
    //                neo4j_s.Del_node(edit.id.ToString());
    //            }
    //    }
    //    public string Ad_user_id(long id_ad)
    //    {
    //        try
    //        {
    //            return query("Match (a:User)-->(n:ad) where id(n)=" + id_ad.ToString() + " return a.Id").Select(a => a["a.Id"]).First().As<string>();
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }
    //}

    //public class Ad_model
    //{

    //    [Required]
    //    [Display(Name = "data")]
    //    [DataType(DataType.MultilineText)]
    //    public string data { get; set; }
    //    [Required]
    //    [Display(Name = "header")]
    //    public string header { get; set; }
    //    [Required]
    //    [Display(Name = "type")]
    //    public string type { get; set; }
    //    [Required]
    //    [Display(Name = "tags")]
    //    public string tags { get; set; }


    //    [Required]
    //    [Display(Name = "Price")]
    //    [DataType(DataType.Currency)]
    //    [DisplayFormat(DataFormatString = "{0:C0}")]
    //    public float Price { get; set; }

    //    public long id { get; set; }

    //    public List<string> tags_list { get; set; }
    //    public string Email { get; set; }
    //}
    //public class tag : neo4j_s
    //{
    //    static Func<IRecord, Tag_model> func_Select;
    //    static tag()
    //    {
    //        func_Select = (a) => new Tag_model { name = ((INode)a["n"])["name"].As<string>(), id = ((INode)a["n"]).Id };
    //    }
    //    public static List<Tag_model> Search_all(string Search, int lim = 25)
    //    {

    //        var test_q = (query("MATCH(n:tag) where n.name =~'.*(?i)" + Search + ".*'  RETURN n LIMIT " + lim + " ").Select(n => new Tag_model { name = ((INode)n["n"])["name"].As<string>(), id = ((INode)n["n"]).Id }).ToList());

    //        return test_q;
    //    }
    //    public static List<Tag_model> tag_id(long id)
    //    {
    //        return query("match (n:tag) where id(n)=" + id + " return n").Select(func_Select).ToList();
    //    }
    //    public static List<INode> tags(long id)
    //    {
    //        return query("match (a:ad)-[:tag]-(n:tag) where id(a)=" + id + " return n").Select(a => (INode)a["n"]).ToList();
    //    }
    //    public static void tag_Add(string tag)
    //    {
    //        query("merge (a:tag{name:'" + tag + "'})");
    //    }
    //    public static void tag_del(string tag)
    //    {
    //        query("match (a:tag{name:'" + tag + "'}) detach delete a");
    //    }
    //    public static List<Tag_model> type_name_rel(string type)
    //    {
    //        return query("match (a:type_standart{name:'" + type + "'})-[:rel]-(n:tag) return n").Select(func_Select).ToList();

    //    }

    //}
    //public class Tag_model
    //{
    //    [Required]
    //    [Display(Name = "name")]
    //    public string name;
    //    public long id;
    //}
    //public class Type_model
    //{
    //    [Required]
    //    [Display(Name = "name")]
    //    public string name;
    //    public long id;
    //    [Required]
    //    [Display(Name = "tags")]
    //    public List<string> tags;

    //}
    //public class type_standart : neo4j_s
    //{

    //    static List<Type_model> name;
    //    public static List<Type_model> get_name_list()
    //    {
    //        return name;
    //    }
    //    static type_standart()
    //    {
    //        name = Type_node_to_list();
    //    }
    //    public static void update()
    //    {
    //        name = Type_node_to_list();

    //    }
    //    public static List<INode> type_standarts()
    //    {
    //        return query("match (n:type_standart) return n").Select(a => (INode)a["n"]).ToList();

    //    }
    //    public static List<INode> types(long id)
    //    {

    //        return query("match (a:ad)-[:type]->(n:type_standart) where id(a)=" + id + " return n").Select(a => (INode)a["n"]).ToList();
    //    }

    //    public static List<Type_model> Type_node_to_list()
    //    {
    //        var inf = type_standarts().Select(a => new Type_model { name = a.Properties["name"].As<string>(), id = a.Id }).ToList();
    //        foreach (var i in inf)
    //        {
    //            i.tags = tag.type_name_rel(i.name).Select(a => a.name).ToList();
    //        }
    //        return inf;
    //    }


    //    public static void type_tag_rel(string type, string tag)
    //    {
    //        query("match (a:type_standart{name:'" + type + "'}),(n:tag{name:'" + tag + "'}) merge (a)-[:rel]->(n)");
    //    }
    //    public static void type_tag_rel_del(string type, string tag)
    //    {
    //        query("match (a:type_standart{name:'" + type + "'})-[k:rel]->(n:tag{name:'" + tag + "'}) delete k");
    //    }

    //    public static void type_Add(string type)
    //    {
    //        query("merge (a:type_standart{name:'" + type + "'})");
    //    }
    //    public static void type_del(string type)
    //    {
    //        query("match (a:type_standart{name:'" + type + "'}) detach delete a");
    //    }

    //}
}