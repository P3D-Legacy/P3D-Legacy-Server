using NUnit.Framework;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Extensions;

using System.Numerics;

namespace P3D.Legacy.Tests
{
    internal sealed class DataItemTests
    {
        [Test]
        public void TestBasic()
        {
            var separator = '.';
            var name = "Aragas";
            var isCorrect = true;
            var idULong = 15124000000000U;
            var @char = 'j' as char?;
            var idInt = 15124;
            var position = Vector3.One;

            var dataItemStorage = new DataItemStorage();
            dataItemStorage.Set(0, name);
            dataItemStorage.Set(1, isCorrect);
            dataItemStorage.Set(2, idULong);
            dataItemStorage.Set(3, @char);
            dataItemStorage.Set(4, idInt);
            dataItemStorage.Set(5, position.ToP3DString(separator));

            Assert.AreEqual(name, dataItemStorage.Get(0));
            Assert.AreEqual(isCorrect, dataItemStorage.GetBool(1));
            Assert.AreEqual(idULong, dataItemStorage.GetUInt64(2));
            Assert.AreEqual(@char, dataItemStorage.GetChar(3));
            Assert.AreEqual(idInt, dataItemStorage.GetInt64(4));
            Assert.AreEqual(position, Vector3Extensions.FromP3DString(dataItemStorage.Get(5), separator));

            Assert.AreEqual("Aragas*1*15124000000000*j*15124*1|1|1", dataItemStorage.ToString());
        }
    }
}