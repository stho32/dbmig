using dbmig.BL.CommandInterpreter;
using dbmig.BL.Database;

namespace dbmig.BL;

public static class InteractorFactory
{
    public static CommandInterpreterInteractor GetCommandInterpreterInteractor()
    {
        return new CommandInterpreterInteractor();
    }

    public static DatabaseInteractor GetDatabaseInteractor()
    {
        return new DatabaseInteractor();
    }
}