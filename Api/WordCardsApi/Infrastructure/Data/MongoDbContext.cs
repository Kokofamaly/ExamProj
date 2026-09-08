using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WordCardsApi.Infrastructure.Settings;
using WordCardsApi.Models;

namespace WordCardsApi.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IPasswordHasher<User> _hasher;

    public MongoDbContext(IOptions<MongoDbSettings> mongoDbSettings, IPasswordHasher<User> hasher)
    {
        var client = new MongoClient(mongoDbSettings.Value.ConnectionURI);
        _database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _hasher = hasher;

        var indexModel = new CreateIndexModel<User>(Builders<User>.IndexKeys.Descending(u => u.Email),
            new CreateIndexOptions { Unique = true });
        Users.Indexes.CreateOne(indexModel);

        CreateSeedInformation();

    }

    private void CreateSeedInformation()
    {

        Console.WriteLine(" SEED CALL ");

        if(Users.CountDocuments(u => true) > 0)
        {
            return;
        }

        Console.WriteLine(" SEEDING STARTED ");
        var user = new User
        {
            Name = "test user",
            Email = "test@gmail.com",
            HashedPassword = "12345"
        };

        user.HashedPassword = _hasher.HashPassword(user, user.HashedPassword);

        Console.WriteLine("INSERTING USER");
        Users.InsertOne(user);

        var userWords = new List<UserWord>();

        string[] pair = "I - я, You - ты или вы, He - он, She - она, It - оно или это, We - мы, They - они, Me - мне или меня, Him - его или ему, Her - ее, Us - нас или нам, Them - их или им, My - мой или моя, Your - твой или ваш, His - его, Our - наш, Their - их, Who - кто, What - что или какой, Person - человек, People - люди, Man - мужчина, Woman - женщина, Child - ребенок, Boy - мальчик, Girl - девочка, Friend - друг, Family - семья, Name - имя, Be - быть, Have - иметь, Do - делать, Say - сказать, Go - идти или ехать, Get - получать или становиться, Make - делать или создавать, Know - знать, Think - думать, Take - брать, See - видеть, Come - приходить, Want - хотеть, Look - смотреть, Use - использовать, Find - находить, Give - давать, Tell - рассказывать, Work - работать, Call - звонить или называть, Try - пытаться, Ask - спрашивать, Need - нуждаться, Feel - чувствовать, Become - становиться, Leave - покидать или уходить, Put - класть или ставить, Mean - иметь в виду или значит, Keep - держать или сохранять, Let - позволять, Begin - начинать, Seem - казаться, Help - помогать, Talk - разговаривать, Turn - поворачивать, Start - начинать, Show - показывать, Hear - слышать, Play - играть, Run - бежать, Move - двигаться, Live - жить, Believe - верить, Bring - приносить, Happen - случаться, Write - писать, Sit - сидеть, Stand - стоять, Lose - терять, Pay - платить, Meet - встречаться, Learn - учиться, Change - менять, Lead - вести или лидировать, Understand - понимать, Watch - смотреть или наблюдать, Follow - следовать, Stop - останавливать, Create - создавать, Speak - говорить, Read - читать, Allow - позволять, Spend - тратить, Time - время, Year - год, Day - день, Week - неделя, Month - месяц, Way - путь или способ, Thing - вещь или предмет, World - мир, Life - жизнь, Hand - рука, Part - часть, Place - место, Case - случай или дело, Government - правительство, Company - компания, Number - число или номер, Group - группа, Problem - проблема, Fact - факт, Eye - глаз, Water - вода, Room - комната, Mother - мать, Father - отец, Area - область или район, Money - деньги, Story - история, Lot - много, Right - право или правая сторона, Study - учеба или исследование, Book - книга, Business - бизнес или дело, Issue - вопрос или проблема, Side - сторона, Kind - вид или тип, Head - голова, House - дом, Service - услуга или служба, Power - сила или власть, Hour - час, Line - линия или строка, End - конец, Game - игра, City - город, Community - сообщество, Good - хороший, New - новый, First - первый, Last - последний, Long - длинный, Great - отличный или великий, Little - маленький, Own - собственный, Other - другой, Old - старый, Big - большой, High - высокий, Different - разный или отличный, Small - маленький, Large - крупный или большой, Next - следующий, Early - ранний, Young - молодой, Important - важный, Few - немногие, Public - публичный или общественный, Bad - плохой, Same - тот же самый, Able - способный, Not - не, Also - также, More - больше, Only - только, Very - очень, Often - часто, Always - всегда, Never - никогда, Well - хорошо, Here - здесь, There - там, When - когда, Why - почему, How - как, Where - где, Again - снова, Together - вместе, Already - уже, Quick - быстро, Now - сейчас, Then - тогда, Out - снаружи или вон, Up - вверх, Down - вниз, About - о или около, Before - до или перед, After - после, Because - потому что, If - если, Or - или, But - но, And - и, With - с, Inside - внутри, Outside - снаружи, Between - между, Under - под".Split(", ");
        string[] words = pair.Select(p => p.Split(" - ")[0]).ToArray();
        string[] translations = pair.Select(p => p.Split(" - ")[1]).ToArray();

        for(int i = 0; i <= 99; i++)
        {
            userWords.Add(new UserWord()
            {
                UserId = user.Id!,
                Word = words[i],
                Translation = translations[i],
                Language = "english",
            });
        }

        string[] foodPairs = "apple - яблоко, banana - банан, orange - апельсин, lemon - лимон, strawberry - клубника, blueberry - черника, grape - виноград, watermelon - арбуз, pineapple - ананас, peach - персик, pear - груша, cherry - вишня, mango - манго, avocado - авокадо, tomato - помидор, potato - картофель, carrot - морковь, onion - лук, garlic - чеснок, cucumber - огурец, cabbage - капуста, broccoli - брокколи, spinach - шпинат, lettuce - салат-латук, pepper - перец, corn - кукуруза, mushroom - гриб, bean - фасоль, pea - горох, rice - рис, bread - хлеб, pasta - макароны, noodle - лапша, flour - мука, cereal - хлопья, oatmeal - овсянка, cheese - сыр, milk - молоко, butter - масло, yogurt - йогурт, cream - сливки, egg - яйцо, chicken - курица, beef - говядина, pork - свинина, lamb - баранина, turkey - индейка, sausage - колбаса, bacon - бекон, steak - стейк, fish - рыба, salmon - лосось, tuna - тунец, shrimp - креветка, crab - краб, soup - суп, salad - салат, sandwich - сэндвич, pizza - пицца, burger - бургер, pancake - блин, waffle - вафля, omelet - омлет, toast - тост, cake - торт, cookie - печенье, biscuit - печенье, chocolate - шоколад, candy - конфета, ice cream - мороженое, pie - пирог, pudding - пудинг, honey - мед, sugar - сахар, salt - соль, spice - специя, sauce - соус, ketchup - кетчуп, mustard - горчица, mayonnaise - майонез, vinegar - уксус, oil - масло, jam - варенье, peanut butter - арахисовая паста, nut - орех, almond - миндаль, walnut - грецкий орех, peanut - арахис, coconut - кокос, raisin - изюм, snack - закуска, meal - прием пищи, breakfast - завтрак, lunch - обед, dinner - ужин, dessert - десерт, ingredient - ингредиент, recipe - рецепт, garlic powder - чесночный порошок, cucumber salad - салат из огурцов, tomato soup - томатный суп, fruit - фрукт, vegetable - овощ, berry - ягода, apple pie - яблочный пирог, cheesecake - чизкейк, cupcake - кекс, muffin - маффин, doughnut - пончик, croissant - круассан, bagel - бублик, sandwich - бутерброд, sausage roll - сосиска в тесте, meatball - фрикаделька, hamburger - гамбургер, hot dog - хот-дог, french fries - картофель фри, mashed potatoes - картофельное пюре, fried egg - жареное яйцо, scrambled eggs - яичница-болтунья, boiled egg - вареное яйцо, fried chicken - жареная курица, roast beef - ростбиф, pork chop - свиная отбивная, lamb chop - баранья отбивная, seafood - морепродукты, octopus - осьминог, squid - кальмар, mussel - мидия, oyster - устрица, lobster - омар, cod - треска, trout - форель, sardine - сардина, anchovy - анчоус, shrimp cocktail - коктейль из креветок, seaweed - морская капуста, lentil - чечевица, chickpea - нут, kidney bean - красная фасоль, soybean - соя, cornmeal - кукурузная мука, wheat - пшеница, barley - ячмень, rye - рожь, buckwheat - гречка, grain - зерно, seed - семя, sunflower seed - семечка подсолнуха, pumpkin seed - тыквенная семечка, cashew - кешью, pistachio - фисташка, hazelnut - фундук, pecan - пекан, peanut - арахис, sesame - кунжут, coconut milk - кокосовое молоко, almond milk - миндальное молоко, fruit juice - фруктовый сок, orange juice - апельсиновый сок, lemonade - лимонад, tea - чай, coffee - кофе, cocoa - какао, smoothie - смузи, milkshake - молочный коктейль, water - вода, sparkling water - газированная вода, jam - варенье, jelly - желе, syrup - сироп, caramel - карамель, vanilla - ваниль, cinnamon - корица, ginger - имбирь, turmeric - куркума, basil - базилик, parsley - петрушка, dill - укроп, mint - мята, rosemary - розмарин, thyme - тимьян, oregano - орегано, chili - перец чили, black pepper - черный перец, sea salt - морская соль, soy sauce - соевый соус, barbecue sauce - соус барбекю, tomato sauce - томатный соус, hot sauce - острый соус, mustard sauce - горчичный соус, olive oil - оливковое масло, sunflower oil - подсолнечное масло, flour - мука, sugar - сахар, yeast - дрожжи, dough - тесто, batter - жидкое тесто, dessert - десерт, appetizer - закуска, main course - основное блюдо".Split(", ");
        string[] foodWords = foodPairs.Select(p => p.Split(" - ")[0]).ToArray();
        string[] foodTranslations = foodPairs.Select(p => p.Split(" - ")[1]).ToArray();

        for(int i = 0; i <= 99; i++)
        {
            userWords.Add(new UserWord()
            {
                UserId = user.Id!,
                Word = foodWords[i],
                Translation = foodTranslations[i],
                Language = "english",
                Category = "food",
                UsageExample = $"I forgot to buy {foodWords[i]}"
            });
        }

        Console.WriteLine("INSERTING WORDS");
        UserWords.InsertMany(userWords);

        LearningSession categorizedSession = new LearningSession
        {
            UserId = user.Id!,
            CreatedAt = DateTimeOffset.UtcNow,
            Category = "food",
            Language = "english"
        };
        LearningSession defaultSession = new LearningSession
        {
            UserId = user.Id!,
            CreatedAt = DateTimeOffset.UtcNow,
        }; 

        Console.WriteLine("INSERTING SESSIONS");
        LearningSessions.InsertOne(categorizedSession);
        LearningSessions.InsertOne(defaultSession);

        List<SessionWord> sessionWords = new List<SessionWord>();

        int index = 0;
        foreach(var word in userWords)
        {
            index++;
            Console.WriteLine($"SESSION WORD NUMBER: {index}");
            sessionWords.Add(new SessionWord
            {
                UserWordId = word.Id!,
                SessionId = word.Category == categorizedSession.Category ? categorizedSession.Id! : defaultSession.Id!,
                Word = word.Word,
                Translation = word.Translation,
                UsageExample = word.UsageExample
            });
        }


        Console.WriteLine("INSERTING SESSIONWORDS");
        SessionWords.InsertMany(sessionWords);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<UserWord> UserWords => _database.GetCollection<UserWord>("userWords");
    public IMongoCollection<LearningSession> LearningSessions => _database.GetCollection<LearningSession>("learningSessions");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("refreshTokens");
    public IMongoCollection<SessionWord> SessionWords => _database.GetCollection<SessionWord>("sessionWords");
}