using Android.Content;
using Android.Database.Sqlite;

namespace Sunshine.Data
{
    public class WeatherDbHelper : SQLiteOpenHelper
    {
        // Manages a local database for weather data.
        // If you change the database schema, you must increment the database version.
        const int DatabaseVersion = 5;
        public const string Database = "weather.db";

        public WeatherDbHelper(Context context)
            : base(context, Database, null, DatabaseVersion)
        {
                
        }

        public override void OnCreate(SQLiteDatabase db)
        {

            const string SqlCreateLocationTable = "CREATE TABLE " +
                                                  WeatherContract.Location.TableName + " (" +
                                                  WeatherContract.Location.Id + " INTEGER PRIMARY KEY AUTOINCREMENT," +
                                                  WeatherContract.Location.ColumnCityName + " TEXT NOT NULL, " +
                                                  WeatherContract.Location.ColumnCoordinationLat + " REAL NOT NULL, " +
                                                  WeatherContract.Location.ColumnCoordinationLong + " REAL NOT NULL, " +
                                                  WeatherContract.Location.ColumnLocationSetting + " TEXT NOT NULL );";

            const string SqlCreateWeatherTable = "CREATE TABLE " +
                                                 WeatherContract.Weather.TableName + " (" +
                // Why AutoIncrement here, and not above?
                // Unique keys will be auto-generated in either case.  But for weather
                // forecasting, it's reasonable to assume the user will want information
                // for a certain date and all dates *following*, so the forecast data
                // should be sorted accordingly.
                                                 WeatherContract.Weather.Id + " INTEGER PRIMARY KEY AUTOINCREMENT," +

                // the ID of the location entry associated with this weather data
                                                 WeatherContract.Weather.ColumnLocationKey + " INTEGER NOT NULL, " +
                                                 WeatherContract.Weather.ColumnDate + " INTEGER NOT NULL, " +
                                                 WeatherContract.Weather.ColumnShortDescription + " TEXT NOT NULL, " +
                                                 WeatherContract.Weather.ColumnWeatherId + " INTEGER NOT NULL," +

                                                 WeatherContract.Weather.ColumnMinimumTemp + " REAL NOT NULL, " +
                                                 WeatherContract.Weather.ColumnMaximumTemp + " REAL NOT NULL, " +

                                                 WeatherContract.Weather.ColumnHumidity + " REAL NOT NULL, " +
                                                 WeatherContract.Weather.ColumnPressure + " REAL NOT NULL, " +
                                                 WeatherContract.Weather.ColumnWindSpeed + " REAL NOT NULL, " +
                                                 WeatherContract.Weather.ColumnDegrees + " REAL NOT NULL, " +

                // Set up the location column as a foreign key to location table.
                                                 " FOREIGN KEY (" + WeatherContract.Weather.ColumnLocationKey + ") REFERENCES " +
                                                 WeatherContract.Location.TableName + " (" + WeatherContract.Location.Id + "), " +

                // To assure the application have just one weather entry per day
                // per location, it's created a UNIQUE constraint with REPLACE strategy
                                                 " UNIQUE (" + WeatherContract.Weather.ColumnDate + ", " +
                                                 WeatherContract.Weather.ColumnLocationKey + ") ON CONFLICT REPLACE);";
            db.ExecSQL(SqlCreateLocationTable);
            db.ExecSQL(SqlCreateWeatherTable);
        }


        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            // This database is only a cache for online data, so its upgrade policy is
            // to simply to discard the data and start over
            // Note that this only fires if you change the version number for your database.
            // It does NOT depend on the version number for your application.
            // If you want to update the schema without wiping data, commenting out the next 2 lines
            // should be your top priority before modifying this method.
            // db.ExecSQL("DROP TABLE IF EXISTS " + WeatherContract.Location.TableName);
            db.ExecSQL("DROP TABLE IF EXISTS " + WeatherContract.Weather.TableName);
            OnCreate(db);
        }
    }
}

