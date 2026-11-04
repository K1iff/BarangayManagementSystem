using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayLangkiwaSystem.Models;

namespace BarangayLangkiwaSystem.Data
{
    public static class MockEntityExtensions
    {
        public static Task Update<T>(this T entity) where T : class
        {
            // For in-memory objects, properties are already modified on reference
            return Task.CompletedTask;
        }
    }

    public class MockQuery<T> where T : class
    {
        private readonly List<T> _source;
        private readonly Func<T, bool>? _predicate;

        public MockQuery(List<T> source, Func<T, bool>? predicate = null)
        {
            _source = source;
            _predicate = predicate;
        }

        public MockQuery<T> Where(Func<T, bool> predicate)
        {
            if (_predicate == null)
            {
                return new MockQuery<T>(_source, predicate);
            }
            var prev = _predicate;
            return new MockQuery<T>(_source, x => prev(x) && predicate(x));
        }

        public Task<MockQueryResponse<T>> Get()
        {
            List<T> list;
            lock (_source)
            {
                list = _predicate != null ? _source.Where(_predicate).ToList() : _source.ToList();
            }
            return Task.FromResult(new MockQueryResponse<T>(list));
        }

        public Task Insert(T item)
        {
            lock (_source)
            {
                _source.Add(item);
            }
            return Task.CompletedTask;
        }

        public Task Delete()
        {
            if (_predicate != null)
            {
                lock (_source)
                {
                    _source.RemoveAll(x => _predicate(x));
                }
            }
            return Task.CompletedTask;
        }
    }

    public class MockQueryResponse<T>
    {
        public List<T> Models { get; }

        public MockQueryResponse(List<T> models)
        {
            Models = models;
        }
    }

    public static class InMemoryDataStore
    {
        public static readonly List<Resident> Residents = new();
        public static readonly List<CertificationLog> CertificationLogs = new();
        public static readonly List<CertificationType> CertificationTypes = new();
        public static readonly List<ReportLog> ReportLogs = new();
        public static readonly List<SystemActivity> SystemActivities = new();
        public static readonly List<UserAccounts> UserAccounts = new();
        public static readonly List<CertRequest> CertificateRequests = new();

        static InMemoryDataStore()
        {
            // Seed default admin account
            UserAccounts.Add(new UserAccounts
            {
                user_id = 1,
                username = "admin",
                password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                staff_name = "System Administrator",
                role = "Barangay Secretary",
                is_active = true
            });

            // Seed sample certification types
            CertificationTypes.Add(new CertificationType { TypeId = 1, TypeName = "Barangay Clearance" });
            CertificationTypes.Add(new CertificationType { TypeId = 2, TypeName = "Certificate of Indigency" });
            CertificationTypes.Add(new CertificationType { TypeId = 3, TypeName = "Certificate of Residency" });
        }

        public static MockQuery<T> From<T>() where T : class
        {
            if (typeof(T) == typeof(Resident)) return new MockQuery<T>((List<T>)(object)Residents);
            if (typeof(T) == typeof(CertificationLog)) return new MockQuery<T>((List<T>)(object)CertificationLogs);
            if (typeof(T) == typeof(CertificationType)) return new MockQuery<T>((List<T>)(object)CertificationTypes);
            if (typeof(T) == typeof(ReportLog)) return new MockQuery<T>((List<T>)(object)ReportLogs);
            if (typeof(T) == typeof(SystemActivity)) return new MockQuery<T>((List<T>)(object)SystemActivities);
            if (typeof(T) == typeof(UserAccounts)) return new MockQuery<T>((List<T>)(object)UserAccounts);
            if (typeof(T) == typeof(CertRequest)) return new MockQuery<T>((List<T>)(object)CertificateRequests);

            return new MockQuery<T>(new List<T>());
        }
    }
}
