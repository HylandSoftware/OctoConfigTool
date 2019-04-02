namespace OctoConfig.Core
{
	public class SecretVariable
	{
		public SecretVariable(string name, string value)
		{
			Name = name;
			Value = value;
		}
		public string Name { get; set; }
		public string Value { get; set; }
		public bool IsSecret { get; set; } = false;

		public override string ToString()
		{
			return $"{Name}={Value}";
		}
	}
}
