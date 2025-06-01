using NUnit.Framework;
using dbmig.BL;
using dbmig.BL.CommandInterpreter;
using dbmig.BL.Database;

namespace dbmig.BL.Tests;

[TestFixture]
public class InteractorFactoryTests
{
    [Test]
    public void GetCommandInterpreterInteractor_ReturnsValidInstance()
    {
        var interactor = InteractorFactory.GetCommandInterpreterInteractor();

        Assert.That(interactor, Is.Not.Null);
        Assert.That(interactor, Is.InstanceOf<CommandInterpreterInteractor>());
    }

    [Test]
    public void GetDatabaseInteractor_ReturnsValidInstance()
    {
        var interactor = InteractorFactory.GetDatabaseInteractor();

        Assert.That(interactor, Is.Not.Null);
        Assert.That(interactor, Is.InstanceOf<DatabaseInteractor>());
    }

    [Test]
    public void GetCommandInterpreterInteractor_ReturnsNewInstanceEachTime()
    {
        var interactor1 = InteractorFactory.GetCommandInterpreterInteractor();
        var interactor2 = InteractorFactory.GetCommandInterpreterInteractor();

        Assert.That(interactor1, Is.Not.SameAs(interactor2));
    }

    [Test]
    public void GetDatabaseInteractor_ReturnsNewInstanceEachTime()
    {
        var interactor1 = InteractorFactory.GetDatabaseInteractor();
        var interactor2 = InteractorFactory.GetDatabaseInteractor();

        Assert.That(interactor1, Is.Not.SameAs(interactor2));
    }
}